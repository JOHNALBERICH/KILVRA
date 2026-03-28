insert into dbo.Categories (Name, Description, CreatedAt) values 
('T-Shirts', 'A variety of stylish and comfortable t-shirts for all occasions.', GETDATE()),
('Jeans', 'Trendy and durable', GETDATE()),
('Dresses', 'Elegant and fashionable dresses for every season.', GETDATE()),
('Shoes', 'Wide selection of', GETDATE()),
('Accessories', 'Complete your look with our range of accessories.', GETDATE());

insert into dbo.Users (FullName, Email, PasswordHash, Phone, Address, Role, CreatedAt, Provider) values 
('John Doe', 'Johndoe@gmail.com', '123', '1234567890', '123 Main Street, Anytown', 'Admin', GETDATE(), 'Local');

insert into dbo.Admins (UserID, Position) values 
(1, 'Admin');

insert into dbo.Shops (AdminID, ShopName, Description, Location, CreatedAt) values 
(1, 'Fashion Hub', 'Your one-stop shop for the latest fashion trends.', '123 Fashion Street, Style City', GETDATE());

insert into dbo.Products (ShopID, Name, Description, Price, Quantity, ImageURL, CreatedAt, Size, CategoryId) values 
(1, 'Classic White T-Shirt', 'A timeless white t-shirt made from soft cotton.', 19.99, 100, 'https://m.media-amazon.com/images/I/51xVDi1XVEL._AC_SY550_.jpg', GETDATE(), 'M', 1),
(1, 'Blue Denim Jeans', 'Stylish blue denim jeans with a comfortable fit.', 49.99, 50, 'https://m.media-amazon.com/images/I/71HVSFd9fyL._AC_SY741_.jpg', GETDATE(), '32', 2),
(1, 'Floral Summer Dress', 'Light and breezy floral dress perfect for summer.', 39.99, 30, 'https://m.media-amazon.com/images/I/71dMuH8LGLL._AC_SX425_.jpg', GETDATE(), 'S', 3),
(1, 'Black Leather Shoes', 'Elegant black leather shoes for formal occasions.', 89.99, 20, 'https://m.media-amazon.com/images/I/71hMm6-AZlL._AC_SY675_.jpg', GETDATE(), '42', 4),
(1, 'Silver Hoop Earrings', 'Classic silver hoop earrings to complement any outfit.', 14.99, 200, 'https://m.media-amazon.com/images/I/6188zfuKlVL._AC_SY535_.jpg', GETDATE(), NULL, 5);