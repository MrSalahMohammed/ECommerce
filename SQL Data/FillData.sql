use ECommerceDB;
go

insert into Countries
Values
('Egypt');

insert into Cities
Values
(1, 'Portsaid'),
(1, 'Cairo'),
(1, 'Alexandria');

insert into OrdersStatus
Values ('Pending'), ('Completed'), ('Cancelled');

insert into PaymentStatus
Values ('Pending'), ('Completed'), ('Failed');

insert into PaymentMethod
Values ('Cash'), ('InstaPay'), ('MasterCard');

insert into Users
Values
('Salah', 'Mohammed', 'Salah.jobacc@gmail.com', 'PlaceHolder', 1, 'Admin', SYSDATETIME()),
('Alaa', 'Mohammed', 'Alaa.jobacc@gmail.com', 'PlaceHolder', 1, 'Customer', SYSDATETIME()),
('Asmaa', 'Mohammed', 'Asmaa.jobacc@gmail.com', 'PlaceHolder', 1, 'Customer', SYSDATETIME());

insert into Addresses
Values
(1, 'Deliver Here', 2),
(1, 'Deliver Here', 2),
(1, 'Deliver Here', 3);

Insert Into ReceiveType
Values
('TakeAway', 0),
('Delivery', 5);

insert into Products
values
('Milk', 'Just a Milk', 2, 5, 1, SYSDATETIME()),
('Cheese', 'Just a Cheese', 3, 5, 1, SYSDATETIME()),
('Chair', 'Just a Chair', 15, 5, 1, SYSDATETIME());

Insert into Orders 
Values
(3, 15, 2, 1, SYSDATETIME());

insert into OrderItems 
Values
(1, 3, 1, 15);

Insert into Payments
Values
(1, 2, 0, 20, 2, 1, SYSDATETIME());
