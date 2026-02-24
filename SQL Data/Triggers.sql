use ECommerceDB
go

CREATE TRIGGER trg_UpdateOrderTotal ON OrderItems
AFTER UPDATE, INSERT, DELETE
AS
BEGIN
	SET NOCOUNT ON;

    UPDATE o
    SET o.TotalAmount = (
        SELECT ISNULL(SUM(oi.Quantity * oi.PriceAtPurchase), 0)
        FROM OrderItems oi
        WHERE oi.OrderID = o.OrderID
    )
    FROM Orders o
    WHERE o.OrderID IN (
        SELECT OrderID FROM inserted
        UNION
        SELECT OrderID FROM deleted
    );
END;
go

CREATE TRIGGER trg_ApplyDiscountToPayment ON Payments
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(TotalAmount) AND NOT UPDATE(Discount) AND NOT UPDATE(OrderID)
        RETURN;

    UPDATE p
    SET p.TotalAmount = 
        o.TotalAmount 
        + ISNULL(rt.Fees, 0)
        - (o.TotalAmount * ISNULL(i.Discount, 0) / 100)
    FROM Payments p
    JOIN inserted i ON p.PaymentID = i.PaymentID
    JOIN Orders o ON i.OrderID = o.OrderID
    JOIN ReceiveType rt ON i.ReceiveTypeID = rt.ReceiveID;  -- was p.ReceiveTypeID, now i.ReceiveTypeID
END;
