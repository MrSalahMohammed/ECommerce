use ECommerceDB;
go

CREATE VIEW vw_ProfitReport AS
SELECT
    o.OrderID,
    o.CreatedAt,
    SUM(oi.Quantity * oi.PriceAtPurchase)                    AS Revenue,
    SUM(oi.Quantity * p.CostPrice)                           AS Cost,
    SUM(oi.Quantity * (oi.PriceAtPurchase - p.CostPrice))    AS Profit
FROM Orders o
JOIN OrderItems oi ON o.OrderID = oi.OrderID
JOIN Products p ON oi.ProductID = p.ProductID
JOIN OrdersStatus os ON o.StatusID = os.StatusID
WHERE os.StatusName = 'Completed'
GROUP BY o.OrderID, o.CreatedAt;
go

Create VIEW vw_Addresses AS
SELECT
    a.AddressID,
	a.CityID,
    a.UserID,
    co.CountryName,
    ci.CityName,
    a.AdditionalInfo
FROM Addresses a
JOIN Cities   ci ON a.CityID     = ci.CityID
JOIN Countries co ON ci.CountryID = co.CountryID;
GO