
--create database ECommerceDB;

use ECommerceDB;

create Table Countries (
	CountryID INT Identity(1,1) Primary Key,
	CountryName nvarchar(50) NOT NULL UNIQUE
);

create Table Cities (
	CityID INT Identity(1,1) Primary Key,
	CountryID INT References Countries(CountryID) NOT NULL,
	CityName nvarchar(50) NOT NULL UNIQUE
);

create table Users (
	PersonID INT Identity(1,1) Primary Key,
	FirstName nvarchar(20) NOT NULL,
	LastName nvarchar(20) NOT NULL,
	Email nvarchar(200) NOT NULL UNIQUE,
	PasswordHash nvarchar(128) NOT NULL,
	isActive BIT NOT NULL DEFAULT 1,
	PersonRole nvarchar(50) NOT NULL Check(PersonRole IN ('Customer', 'Admin')) DEFAULT 'Customer',
	CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

create Table Addresses (
	AddressID INT Identity(1,1) Primary Key,
	CityID INT References Cities(CityID) NOT NULL,
	AdditionalInfo nvarchar(200) NOT NULL,
	UserID INT References Users(PersonID) NOT NULL
);

create Table Products (
	ProductID INT Identity(1,1) Primary Key,
	ProductName nvarchar(150) NOT NULL,
	ProductDescription nvarchar(200) NOT NULL,
	Price DECIMAL(18,2) NOT NULL Check(Price > 0),
	StockQuantity INT NOT NULL Check(StockQuantity >= 0),
	isActive BIT NOT NULL DEFAULT 1,
	CreatedAt DateTime2 NOT NULL DEFAULT SYSDATETIME()
);

ALTER TABLE Products ADD CostPrice DECIMAL(18,2) NOT NULL CHECK(CostPrice > 0) DEFAULT 0;

Create INDEX IX_ProductName ON Products(ProductName);

create Table OrdersStatus (
	StatusID INT Identity(1,1) Primary Key, 
	StatusName nvarchar(50) NOT NULL UNIQUE
);

Create Table Orders (
	OrderID INT Identity(1,1) Primary Key, 
	UserID INT References Users(PersonID) NOT NULL,
	TotalAmount DECIMAL(18,2) NOT NULL Check(TotalAmount >= 0),
	StatusID INT REFERENCES OrdersStatus(StatusID) NOT NULL,
	ShippingAddressID INT References Addresses(AddressID) NOT NULL,
	CreatedAt DateTime2 NOT NULL DEFAULT SYSDATETIME()
);

Create INDEX IX_OrderUserID ON Orders(UserID);

Create Table OrderItems(
	OrderItemID INT Identity(1,1) Primary Key, 
	OrderID INT References Orders(OrderID) ON DELETE CASCADE NOT NULL,
	ProductID INT References Products(ProductID) NOT NULL,
	Quantity INT NOT NULL Check(Quantity > 0),
	PriceAtPurchase DECIMAL(18,2) NOT NULL Check(PriceAtPurchase > 0)
);

Create INDEX IX_OrderItems_OrderID ON OrderItems(OrderID);
Create INDEX IX_OrderItems_ProductID ON OrderItems(ProductID);


create Table PaymentMethod (
	MethodID INT Identity(1,1) PRIMARY KEY,
	MethodName nvarchar(50) NOT NULL UNIQUE
);

create Table ReceiveType (
	ReceiveID INT Identity(1,1) PRIMARY KEY,
	TypeName nvarchar(50) NOT NULL UNIQUE,
	Fees DECIMAL(18,2) NOT NULL
);

create Table PaymentStatus (
	StatusID INT Identity(1,1) Primary Key, 
	StatusName nvarchar(50) NOT NULL UNIQUE
);

Create Table Payments (
	PaymentID INT Identity(1,1) Primary Key, 
	OrderID INT References Orders(OrderID) NOT NULL UNIQUE,
	ReceiveTypeID INT References ReceiveType(ReceiveID) NOT NULL,
	Discount DECIMAL(18, 2) Check(Discount >= 0 AND Discount <= 100) NULL,
	TotalAmount DECIMAL(18,2) NOT NULL Check(TotalAmount > 0),
	PaymentStatusID INT REFERENCES PaymentStatus(StatusID) NOT NULL,
	PaymentMethodID INT REFERENCES PaymentMethod(MethodID) NOT NULL,
	PaymentDate DateTime2 NULL
);
