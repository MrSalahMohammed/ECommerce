use ECommerceDB;
go


CREATE TYPE OrderItemType AS TABLE (
    ProductID INT NOT NULL,
    Quantity  INT NOT NULL
);
GO

-- Place Order
CREATE PROCEDURE SP_PlaceOrder
    @UserID            INT,
    @ShippingAddressID INT,
    @Items             OrderItemType READONLY,
    @NewOrderID        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Items list must not be empty
        IF NOT EXISTS (SELECT 1 FROM @Items)
            THROW 50001, 'Order must contain at least one item.', 1;

        -- 2. Validate user exists and is active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE PersonID = @UserID AND isActive = 1)
            THROW 50002, 'Invalid or inactive user.', 1;

        -- 3. Validate shipping address belongs to user
        IF NOT EXISTS (SELECT 1 FROM Addresses WHERE AddressID = @ShippingAddressID AND UserID = @UserID)
            THROW 50003, 'Shipping address does not belong to this user.', 1;

        -- 4. Validate all products exist and are active
        IF EXISTS (
            SELECT 1 FROM @Items i
            LEFT JOIN Products p ON i.ProductID = p.ProductID
            WHERE p.ProductID IS NULL OR p.isActive = 0
        )
            THROW 50004, 'One or more products are invalid or inactive.', 1;

        -- 5. Validate stock availability
        IF EXISTS (
            SELECT 1 FROM @Items i
            JOIN Products p ON i.ProductID = p.ProductID
            WHERE p.StockQuantity < i.Quantity
        )
            THROW 50005, 'Insufficient stock for one or more products.', 1;


        DECLARE @PendingStatusID INT = (
            SELECT StatusID FROM OrdersStatus WHERE StatusName = 'Pending'
        );

        IF @PendingStatusID IS NULL
            THROW 50006, 'Pending status not found in OrdersStatus table.', 1;


        INSERT INTO Orders (UserID, TotalAmount, StatusID, ShippingAddressID)
        VALUES (@UserID, 0, @PendingStatusID, @ShippingAddressID);

        SET @NewOrderID = SCOPE_IDENTITY();


        INSERT INTO OrderItems (OrderID, ProductID, Quantity, PriceAtPurchase)
        SELECT
            @NewOrderID,
            p.ProductID,
            i.Quantity,
            p.Price
        FROM @Items i
        JOIN Products p ON i.ProductID = p.ProductID;


        UPDATE p
        SET p.StockQuantity = p.StockQuantity - i.Quantity
        FROM Products p
        JOIN @Items i ON p.ProductID = i.ProductID
        WHERE p.StockQuantity >= i.Quantity;  -- re-check at update time

        -- If fewer rows updated than items, stock changed mid-transaction
        IF @@ROWCOUNT < (SELECT COUNT(*) FROM @Items)
            THROW 50007, 'Stock changed during order placement. Please try again.', 1;

        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Add a New Product
Create PROCEDURE SP_AddProduct
    @ProductName        NVARCHAR(150),
    @ProductDescription NVARCHAR(200),
    @Price              DECIMAL(18,2),
	@CostPrice          DECIMAL(18,2),
    @StockQuantity      INT,
    @NewProductID       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -- 1. Validate name is not empty
        IF LTRIM(RTRIM(@ProductName)) = ''
            THROW 50001, 'Product name cannot be empty.', 1;

        -- 2. Validate price
        IF @Price <= 0
            THROW 50002, 'Price must be greater than 0.', 1;

		IF @CostPrice <= 0
            THROW 50003, 'Cost price must be greater than 0.', 1;

        -- 3. Validate stock
        IF @StockQuantity < 0
            THROW 50004, 'Stock quantity cannot be negative.', 1;

		IF @CostPrice >= @Price
            THROW 50005, 'Cost price must be less than sale price.', 1;

        -- 4. Check for duplicate product name
        IF EXISTS (SELECT 1 FROM Products WHERE ProductName = @ProductName)
            THROW 50006, 'A product with this name already exists.', 1;

        INSERT INTO Products (ProductName, ProductDescription, Price, CostPrice, StockQuantity, isActive)
        VALUES (@ProductName, @ProductDescription, @Price, @CostPrice, @StockQuantity, 1);

        SET @NewProductID = SCOPE_IDENTITY();

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO


-- Add Stock to Existing Product
CREATE PROCEDURE SP_AddStock
    @ProductID    INT,
    @QuantityToAdd INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -- 1. Validate quantity
        IF @QuantityToAdd <= 0
            THROW 50001, 'Quantity to add must be greater than 0.', 1;

        -- 2. Validate product exists and is active
        IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductID = @ProductID AND isActive = 1)
            THROW 50002, 'Product not found or is inactive.', 1;

        UPDATE Products
        SET StockQuantity = StockQuantity + @QuantityToAdd
        WHERE ProductID = @ProductID;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO


-- Cancel Order (restores stock)
CREATE PROCEDURE SP_CancelOrder
    @OrderID INT,
    @UserID  INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Validate order exists and belongs to user
        IF NOT EXISTS (SELECT 1 FROM Orders WHERE OrderID = @OrderID AND UserID = @UserID)
            THROW 50001, 'Order not found or does not belong to this user.', 1;

        -- 2. Make sure order is still Pending (can't cancel Completed/Cancelled)
        DECLARE @CurrentStatusID INT = (SELECT StatusID FROM Orders WHERE OrderID = @OrderID);
        DECLARE @PendingStatusID INT  = (SELECT StatusID FROM OrdersStatus WHERE StatusName = 'Pending');
        DECLARE @CancelledStatusID INT = (SELECT StatusID FROM OrdersStatus WHERE StatusName = 'Cancelled');

        IF @CurrentStatusID != @PendingStatusID
            THROW 50002, 'Only pending orders can be cancelled.', 1;

        -- 3. Restore stock
        UPDATE p
        SET p.StockQuantity = p.StockQuantity + oi.Quantity
        FROM Products p
        JOIN OrderItems oi ON p.ProductID = oi.ProductID
        WHERE oi.OrderID = @OrderID;

        -- 4. Update order status to Cancelled
        UPDATE Orders
        SET StatusID = @CancelledStatusID
        WHERE OrderID = @OrderID;

        -- 5. Update payment status to Failed if a payment exists
        DECLARE @FailedPaymentStatusID INT = (SELECT StatusID FROM PaymentStatus WHERE StatusName = 'Failed');

        UPDATE Payments
        SET PaymentStatusID = @FailedPaymentStatusID
        WHERE OrderID = @OrderID;

        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


-- Process Payment
CREATE PROCEDURE SP_ProcessPayment
    @OrderID         INT,
    @ReceiveTypeID   INT,
    @Discount        DECIMAL(18,2) = NULL,
    @PaymentMethodID INT,
    @NewPaymentID    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Validate order exists and is Pending
        DECLARE @PendingStatusID INT = (SELECT StatusID FROM OrdersStatus WHERE StatusName = 'Pending');

        IF NOT EXISTS (SELECT 1 FROM Orders WHERE OrderID = @OrderID AND StatusID = @PendingStatusID)
            THROW 50001, 'Order not found or is not in Pending status.', 1;

        -- 2. Validate no payment already exists for this order
        IF EXISTS (SELECT 1 FROM Payments WHERE OrderID = @OrderID)
            THROW 50002, 'A payment already exists for this order.', 1;

        -- 3. Validate receive type
        IF NOT EXISTS (SELECT 1 FROM ReceiveType WHERE ReceiveID = @ReceiveTypeID)
            THROW 50003, 'Invalid receive type.', 1;

        -- 4. Validate payment method
        IF NOT EXISTS (SELECT 1 FROM PaymentMethod WHERE MethodID = @PaymentMethodID)
            THROW 50004, 'Invalid payment method.', 1;

        -- 5. Validate discount range
        IF @Discount IS NOT NULL AND (@Discount < 0 OR @Discount > 100)
            THROW 50005, 'Discount must be between 0 and 100.', 1;

        -- 6. Get Pending payment status
        DECLARE @PendingPaymentStatusID INT = (SELECT StatusID FROM PaymentStatus WHERE StatusName = 'Pending');

        IF @PendingPaymentStatusID IS NULL
            THROW 50006, 'Pending payment status not found.', 1;

        -- 7. Insert payment (TotalAmount set to 0, trg_ApplyDiscountToPayment handles it)
        INSERT INTO Payments (OrderID, ReceiveTypeID, Discount, TotalAmount, PaymentStatusID, PaymentMethodID, PaymentDate)
        VALUES (@OrderID, @ReceiveTypeID, @Discount, 0, @PendingPaymentStatusID, @PaymentMethodID, SYSDATETIME());

        SET @NewPaymentID = SCOPE_IDENTITY();

        -- 8. Mark order as Completed
        DECLARE @CompletedStatusID INT = (SELECT StatusID FROM OrdersStatus WHERE StatusName = 'Completed');

        UPDATE Orders
        SET StatusID = @CompletedStatusID
        WHERE OrderID = @OrderID;

        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


-- Register User
CREATE PROCEDURE SP_RegisterUser
    @FirstName    NVARCHAR(20),
    @LastName     NVARCHAR(20),
    @Email        NVARCHAR(50),
    @PasswordHash NVARCHAR(128),
    @NewUserID    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -- 1. Validate fields are not empty
        IF LTRIM(RTRIM(@FirstName)) = '' OR LTRIM(RTRIM(@LastName)) = ''
            THROW 50001, 'First and last name cannot be empty.', 1;

        IF LTRIM(RTRIM(@Email)) = ''
            THROW 50002, 'Email cannot be empty.', 1;

        IF LTRIM(RTRIM(@PasswordHash)) = ''
            THROW 50003, 'Password cannot be empty.', 1;

        -- 2. Validate email is unique
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
            THROW 50004, 'An account with this email already exists.', 1;

        INSERT INTO Users (FirstName, LastName, Email, PasswordHash, isActive, PersonRole)
        VALUES (@FirstName, @LastName, @Email, @PasswordHash, 1, 'Customer');

        SET @NewUserID = SCOPE_IDENTITY();

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

CREATE PROCEDURE SP_LoginUser
    @Email NVARCHAR(50),
    @PasswordHash NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

		IF LTRIM(RTRIM(@Email)) = ''
            THROW 50002, 'Email cannot be empty.', 1;

        IF LTRIM(RTRIM(@PasswordHash)) = ''
            THROW 50003, 'Password cannot be empty.', 1;

        -- 2. Validate email And Password
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND PasswordHash = @PasswordHash)
            THROW 50004, 'Wrong Email or Password', 1;

    SELECT 
        PersonID,
        FirstName,
		LastName,
        Email,
        CreatedAt,
		PersonRole
    FROM Users
    WHERE Email = @Email
      AND PasswordHash = @PasswordHash
      AND IsActive = 1;
END;
GO


-- Deactivate User
CREATE PROCEDURE SP_DeactivateUser
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -- 1. Validate user exists and is already active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE PersonID = @UserID AND isActive = 1)
            THROW 50001, 'User not found or is already inactive.', 1;

        -- 2. Check user has no pending orders
        DECLARE @PendingStatusID INT = (SELECT StatusID FROM OrdersStatus WHERE StatusName = 'Pending');

        IF EXISTS (SELECT 1 FROM Orders WHERE UserID = @UserID AND StatusID = @PendingStatusID)
            THROW 50002, 'Cannot deactivate user with pending orders.', 1;

        UPDATE Users
        SET isActive = 0
        WHERE PersonID = @UserID;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO


-- Update User
Create PROCEDURE SP_UpdateUser
    @UserID       INT,
    @FirstName    NVARCHAR(20)  = NULL,
    @LastName     NVARCHAR(20)  = NULL,
    @Email        NVARCHAR(50)  = NULL,
    @PasswordHash NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -- 1. Validate user exists and is active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE PersonID = @UserID AND isActive = 1)
            THROW 50001, 'User not found or is inactive.', 1;

        -- 2. If email is being changed, check it is not taken
        IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND PersonID != @UserID)
            THROW 50002, 'This email is already in use by another account.', 1;

		-- 3. check if True Password or not
		IF @PasswordHash IS NOT NULL AND EXISTS (SELECT 1 FROM Users WHERE PersonID = @UserID AND PasswordHash = @PasswordHash)
			THROW 50003, 'Wrong Password', 1;

        -- 3. Update only the fields that were passed in
        UPDATE Users
        SET
            FirstName    = ISNULL(@FirstName,    FirstName),
            LastName     = ISNULL(@LastName,     LastName),
            Email        = ISNULL(@Email,        Email)
        WHERE PersonID = @UserID;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO



-- Deactivate Product
CREATE PROCEDURE SP_DeactivateProduct
    @ProductID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -- 1. Validate product exists and is active
        IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductID = @ProductID AND isActive = 1)
            THROW 50001, 'Product not found or is already inactive.', 1;

        UPDATE Products
        SET isActive = 0
        WHERE ProductID = @ProductID;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO


-- Update Product
Create PROCEDURE SP_UpdateProduct
    @ProductID          INT,
    @ProductName        NVARCHAR(150) = NULL,
    @ProductDescription NVARCHAR(200) = NULL,
    @Price              DECIMAL(18,2) = NULL,
	@CostPrice          DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -- 1. Validate product exists and is active
        IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductID = @ProductID AND isActive = 1)
            THROW 50001, 'Product not found or is inactive.', 1;

        -- 2. Validate price if provided
        IF @Price IS NOT NULL AND @Price <= 0
            THROW 50002, 'Price must be greater than 0.', 1;

		IF @CostPrice <= 0
            THROW 50003, 'Cost price must be greater than 0.', 1;

        IF @CostPrice >= @Price
            THROW 50004, 'Cost price must be less than sale price.', 1;

        -- 3. Validate name is not duplicate if provided
        IF @ProductName IS NOT NULL AND EXISTS (
            SELECT 1 FROM Products WHERE ProductName = @ProductName AND ProductID != @ProductID
        )
            THROW 50005, 'A product with this name already exists.', 1;

        UPDATE Products
        SET
            ProductName        = ISNULL(@ProductName,        ProductName),
            ProductDescription = ISNULL(@ProductDescription, ProductDescription),
            Price              = ISNULL(@Price,              Price),
			CostPrice          = ISNULL(@CostPrice,          Price)
        WHERE ProductID = @ProductID;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO


-- Update Order Status
Create PROCEDURE SP_UpdateOrderStatus
    @OrderID     INT,
    @StatusID    NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -- 1. Validate order exists
        IF NOT EXISTS (SELECT 1 FROM Orders WHERE OrderID = @OrderID)
            THROW 50001, 'Order not found.', 1;

        -- 2. Validate status exists
        DECLARE @NewStatusID INT = (SELECT StatusID FROM OrdersStatus WHERE StatusID = StatusID);

        IF @NewStatusID IS NULL
            THROW 50002, 'Invalid order status name.', 1;

        -- 3. Prevent updating a Cancelled order
        DECLARE @CancelledStatusID INT = (SELECT StatusID FROM OrdersStatus WHERE StatusName = 'Cancelled');

        IF (SELECT StatusID FROM Orders WHERE OrderID = @OrderID) = @CancelledStatusID
            THROW 50003, 'Cannot update status of a cancelled order.', 1;

        UPDATE Orders
        SET StatusID = @NewStatusID
        WHERE OrderID = @OrderID;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO


-- Update Payment Status
CREATE PROCEDURE SP_UpdatePaymentStatus
    @PaymentID   INT,
    @StatusName  NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -- 1. Validate payment exists
        IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentID = @PaymentID)
            THROW 50001, 'Payment not found.', 1;

        -- 2. Validate status exists
        DECLARE @NewStatusID INT = (SELECT StatusID FROM PaymentStatus WHERE StatusName = @StatusName);

        IF @NewStatusID IS NULL
            THROW 50002, 'Invalid payment status name.', 1;

        -- 3. Prevent updating a Failed payment
        DECLARE @FailedStatusID INT = (SELECT StatusID FROM PaymentStatus WHERE StatusName = 'Failed');

        IF (SELECT PaymentStatusID FROM Payments WHERE PaymentID = @PaymentID) = @FailedStatusID
            THROW 50003, 'Cannot update status of a failed payment.', 1;

        UPDATE Payments
        SET PaymentStatusID = @NewStatusID
        WHERE PaymentID = @PaymentID;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO



-- Add Address
CREATE PROCEDURE SP_AddAddress
    @UserID         INT,
    @CityID         INT,
    @AdditionalInfo NVARCHAR(200),
    @NewAddressID   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -- 1. Validate user exists and is active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE PersonID = @UserID AND isActive = 1)
            THROW 50001, 'User not found or is inactive.', 1;

        -- 2. Validate city exists
        IF NOT EXISTS (SELECT 1 FROM Cities WHERE CityID = @CityID)
            THROW 50002, 'City not found.', 1;

        -- 3. Validate additional info is not empty
        IF LTRIM(RTRIM(@AdditionalInfo)) = ''
            THROW 50003, 'Additional address info cannot be empty.', 1;

        INSERT INTO Addresses (CityID, AdditionalInfo, UserID)
        VALUES (@CityID, @AdditionalInfo, @UserID);

        SET @NewAddressID = SCOPE_IDENTITY();

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO



-- Remove Address
CREATE PROCEDURE SP_RemoveAddress
    @AddressID INT,
    @UserID    INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -- 1. Validate address exists and belongs to user
        IF NOT EXISTS (SELECT 1 FROM Addresses WHERE AddressID = @AddressID AND UserID = @UserID)
            THROW 50001, 'Address not found or does not belong to this user.', 1;

        -- 2. Prevent deletion if address is linked to any order
        IF EXISTS (SELECT 1 FROM Orders WHERE ShippingAddressID = @AddressID)
            THROW 50002, 'Cannot remove an address that is linked to an existing order.', 1;

        DELETE FROM Addresses
        WHERE AddressID = @AddressID;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

Create PROCEDURE SP_GetAllCustomers
As
BEGIN
	SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

		IF NOT EXISTS (SELECT 1 FROM Users WHERE PersonRole = 'Customer')
            THROW 50001, 'There Are No Customers', 1;

		SELECT PersonID, FirstName, LastName, Email, PersonRole, CreatedAt
		FROM Users WHERE (PersonRole = 'Customer' AND isActive = 1);

	END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

Create PROCEDURE SP_MakeAdmin
	@newUserID   INT,
	@CurrentUserID INT,
	@PasswordHash NVARCHAR(128)

AS
BEGIN
	SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
		-- 1. Validate NewUser exists and is active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE PersonID = @newUserID AND isActive = 1)
            THROW 50001, 'User not found or is inactive.', 1;

		-- 1. Validate Admin exists and is active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE PersonID = @CurrentUserID AND isActive = 1 AND PersonRole = 'Admin')
            THROW 50001, 'Admin not found or is inactive.', 1;


		-- 3. check if True Password or not
		IF @PasswordHash IS NOT NULL AND EXISTS (SELECT 1 FROM Users WHERE PersonID = @CurrentUserID AND PasswordHash = @PasswordHash)
			THROW 50002, 'Wrong Password', 1;

        -- 3. Update only the fields that were passed in
        UPDATE Users
        SET
            PersonRole = 'Admin'
        WHERE PersonID = @newUserID;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

CREATE PROCEDURE SP_LoginUser
    @Email        NVARCHAR(50),
    @PasswordHash NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        -- 1. Validate inputs
        IF LTRIM(RTRIM(@Email)) = ''
            THROW 50001, 'Email cannot be empty.', 1;

        IF LTRIM(RTRIM(@PasswordHash)) = ''
            THROW 50002, 'Password cannot be empty.', 1;

        -- 2. Validate user exists and is active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND PasswordHash = @PasswordHash AND isActive = 1)
            THROW 50003, 'Invalid email or password.', 1;

        -- 3. Return user data
        SELECT
            PersonID,
            FirstName,
            LastName,
            Email,
            PersonRole
        FROM Users
        WHERE Email        = @Email
          AND PasswordHash = @PasswordHash
          AND isActive     = 1;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

Create PROCEDURE SP_GetUserAllAddresses
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY

        -- 1. Validate user exists and is active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE PersonID = @UserID AND isActive = 1)
            THROW 50001, 'User not found or is inactive.', 1;

        -- 2. Validate addresses exist
        IF NOT EXISTS (SELECT 1 FROM Addresses WHERE UserID = @UserID)
            THROW 50002, 'No addresses found for this user.', 1;

        -- 3. Return clean readable result
        SELECT AddressID, CityID, CountryName, CityName, AdditionalInfo
        FROM vw_Addresses
        WHERE UserID = @UserID;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

Create PROCEDURE SP_GetOrdersByUser
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

	-- 1. Validate user exists and is active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE PersonID = @UserID AND isActive = 1)
            THROW 50001, 'User not found or is inactive.', 1;

	-- 2. Validate order exists
        IF NOT EXISTS (SELECT 1 FROM Orders WHERE UserID = @UserID)
            THROW 50001, 'User Has No Orders', 1;

    SELECT 
        OrderID,
        UserID,
        TotalAmount,
        StatusName,
		CreatedAt
    FROM vw_Orders
    WHERE UserID = @UserID
    ORDER BY OrderID DESC;
END;
GO

Create PROCEDURE SP_GetOrderDetails
    @OrderID INT
AS
BEGIN
    SET NOCOUNT ON;

	-- 1. Validate order exists
        IF NOT EXISTS (SELECT 1 FROM Orders WHERE OrderID = @OrderID)
            THROW 50001, 'User Has No Orders', 1;

    SELECT 
        o.OrderID,
        o.UserID,
        o.TotalAmount,
        os.StatusName,
        oi.ProductID,
        p.ProductName,
        oi.Quantity,
        oi.PriceAtPurchase,
		o.CreatedAt
    FROM Orders o
    INNER JOIN OrdersStatus os ON o.StatusID = os.StatusID
    INNER JOIN OrderItems oi ON o.OrderID = oi.OrderID
    INNER JOIN Products p ON oi.ProductID = p.ProductID
    WHERE o.OrderID = @OrderID;
END;
GO
