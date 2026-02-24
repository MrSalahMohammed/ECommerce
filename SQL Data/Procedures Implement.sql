use ECommerceDB;
go

-- Place Order
DECLARE @OrderID INT;
DECLARE @MyItems OrderItemType;

INSERT INTO @MyItems VALUES (1, 1), (3, 4);  -- ProductID, Quantity

EXEC SP_PlaceOrder
    @UserID            = 3,
    @ShippingAddressID = 3,
    @Items             = @MyItems,
    @NewOrderID        = @OrderID OUTPUT;

SELECT @OrderID AS NewOrderID;
go

-- Add a new product
DECLARE @ProductID INT;

EXEC SP_AddProduct
    @ProductName        = 'Water Bottle',
    @ProductDescription = 'Just a Water Bottle',
    @Price              = 5.00,
    @StockQuantity      = 100,
    @NewProductID       = @ProductID OUTPUT;

SELECT @ProductID AS NewProductID;




-- Add stock to an existing product
EXEC SP_AddStock
    @ProductID     = 3,
    @QuantityToAdd = 50;


Select * From Products;


-- Cancel an order
EXEC SP_CancelOrder
    @OrderID = 1,
    @UserID  = 3;

-- Process a payment (10% discount, Delivery, MasterCard)
DECLARE @PaymentID INT;

EXEC SP_ProcessPayment
    @OrderID         = 1,
    @ReceiveTypeID   = 2,
    @Discount        = 10,
    @PaymentMethodID = 3,
    @NewPaymentID    = @PaymentID OUTPUT;

SELECT @PaymentID AS NewPaymentID;

-- Register a new user
DECLARE @UserID INT;
EXEC SP_RegisterUser 'Ahmed', 'Mohammed', 'Ahmed@email.com', 'Test', @UserID OUTPUT;
SELECT @UserID AS NewUserID;

-- Update only email (other fields stay unchanged)
EXEC SP_UpdateUser @UserID = 1, @Email = 'newemail@email.com';

-- Deactivate a user
EXEC SP_DeactivateUser @UserID = 1;

-- Update product price only
EXEC SP_UpdateProduct @ProductID = 1, @Price = 9.99;

-- Deactivate a product
EXEC SP_DeactivateProduct @ProductID = 2;

-- Update order status
EXEC SP_UpdateOrderStatus @OrderID = 1, @StatusName = 'Completed';

-- Update payment status
EXEC SP_UpdatePaymentStatus @PaymentID = 1, @StatusName = 'Completed';

-- Add a new address
DECLARE @AddressID INT;
EXEC SP_AddAddress @UserID = 2, @CityID = 1, @AdditionalInfo = 'Street 5, Building 3', @NewAddressID = @AddressID OUTPUT;
SELECT @AddressID AS NewAddressID;

-- Remove an address
EXEC SP_RemoveAddress @AddressID = 1, @UserID = 2;