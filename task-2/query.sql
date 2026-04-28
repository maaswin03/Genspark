--BASIC

--List all customers from USA
SELECT * FROM customers WHERE country = 'USA';

--List all products where UnitPrice is greater than 20
SELECT * FROM products WHERE unitprice > 20;

--List all orders placed after 1997-01-01
SELECT * FROM orders WHERE orderdate > '1997-01-01';

--Display customers ordered by Country and then CompanyName
SELECT * FROM customers ORDER BY country , companyname;

--List products ordered by highest UnitPrice first
SELECT * FROM products ORDER BY unitprice DESC;


--Group By

--Count how many customers are there in each country
SELECT country , COUNT(*) AS total_customers FROM customers GROUP BY country;

--Find the number of products in each category
SELECT c.categoryname , COUNT(p.productid) AS total_products FROM products p 
INNER JOIN categories c ON p.categoryid = c.categoryid
GROUP BY c.categoryname;

--Find the total number of orders handled by each employee
SELECT o.employeeid , e.lastname , e.firstname , e.title ,COUNT(o.employeeid) AS total_orders FROM orders o
INNER JOIN employees e ON o.employeeid = e.employeeid
GROUP BY o.employeeid , e.lastname , e.firstname , e.title;

--Find the average freight amount for each customer
SELECT o.customerid , c.companyname , ROUND(AVG(o.freight),2) AS average_freight FROM orders o
INNER JOIN customers c ON o.customerid = c.customerid
GROUP BY o.customerid, c.companyname;

--Find the maximum unit price in each category
SELECT p.categoryid , c.categoryname , MAX(p.unitprice) AS maximum_unit FROM products p 
INNER JOIN categories c ON c.categoryid = p.categoryid
GROUP BY p.categoryid ,c.categoryname;


--Having

--Show countries having more than 5 customers
SELECT country , COUNT(*) AS number_of_customers FROM customers GROUP BY country HAVING COUNT(*) > 5;

--Show employees who handled more than 50 orders
SELECT o.employeeid , e.lastname , e.firstname , e.title , COUNT(*) AS orders_handled FROM orders o
INNER JOIN employees e ON o.employeeid = e.employeeid
GROUP BY o.employeeid ,e.lastname , e.firstname , e.title HAVING COUNT(*) > 50;

--Show customers whose average freight is greater than 50
SELECT o.customerid , c.companyname ,  ROUND(AVG(o.freight),2) AS avg_freight FROM orders o 
INNER JOIN customers c ON o.customerid = c.customerid
GROUP BY o.customerid , c.companyname HAVING ROUND(AVG(o.freight),2) > 50;

--Show categories where the average product price is greater than 30
SELECT c.categoryname , ROUND(AVG(p.unitprice),2) AS total_products FROM products p 
INNER JOIN categories c ON p.categoryid = c.categoryid
GROUP BY c.categoryname HAVING ROUND(AVG(p.unitprice),2) > 30;

--Show ship countries having more than 20 orders
SELECT shipcountry , COUNT(*) AS total_orders FROM orders GROUP BY shipcountry HAVING COUNT(*) > 20;


--Joins

--List each order with customer company name
SELECT o.orderid , o.orderdate , o.requireddate , o.freight , c.companyname , c.contactname  FROM orders o 
INNER JOIN customers c ON o.customerid = c.customerid;

--List each order with employee first name and last name
SELECT o.orderid , o.orderdate , e.lastname , e.firstname , e.title FROM orders o 
INNER JOIN employees e ON o.employeeid = e.employeeid;

--List products with their category name
SELECT p.productname , p.quantityperunit , p.unitprice , c.categoryname FROM products p 
INNER JOIN categories c ON p.categoryid = c.categoryid;

--List products with supplier company name
SELECT p.productname , p.quantityperunit , p.unitprice , s.companyname , s.contactname FROM products p
INNER JOIN suppliers s ON p.supplierid = s.supplierid;

--List orders with shipper company name
SELECT o.orderid , o.orderdate , s.companyname , s.phone FROM orders o 
INNER JOIN shippers s ON o.shipvia = s.shipperid;


--Medium

--Find total orders per customer and display customer company name
SELECT o.customerid , c.companyname , c.contactname , COUNT(o.customerid) AS total_orders FROM orders o 
INNER JOIN customers c ON o.customerid = c.customerid
GROUP BY o.customerid , c.companyname , c.contactname ;

--Find total products supplied by each supplier
SELECT s.supplierid , s.companyname , s.contactname , COUNT(p.supplierid) AS total_products FROM suppliers s
INNER JOIN products p ON s.supplierid = p.supplierid
GROUP BY s.supplierid , s.companyname , s.contactname;

--Find average product price per category with category name
SELECT c.categoryname , ROUND(AVG(p.unitprice),2) AS average_product_price FROM categories c
INNER JOIN products p ON c.categoryid = p.categoryid
GROUP BY p.categoryid , c.categoryname;

--Find total freight per customer and order by highest total freight.
SELECT o.customerid , c.companyname , c.contactname , ROUND(SUM(o.freight),2) AS total_freight FROM orders o
INNER JOIN customers c ON o.customerid = c.customerid
GROUP BY o.customerid , c.companyname , c.contactname ORDER BY SUM(o.freight) DESC;

--Find employees who handled more than 25 orders
SELECT o.employeeid , e.lastname , e.firstname , e.title , COUNT(*) AS orders_handled FROM orders o
INNER JOIN employees e ON o.employeeid = e.employeeid
GROUP BY o.employeeid ,e.lastname , e.firstname , e.title HAVING COUNT(*) > 25;


--Advanced

--Find total sales amount per order
SELECT o.orderid , o.orderdate , SUM(d.unitprice *  d.quantity) AS total_sales FROM order_details d 
INNER JOIN orders o ON o.orderid = d.orderid
GROUP BY o.orderid;

--Find total sales amount per customer.
SELECT o.customerid , c.companyname , c.contactname , SUM(d.unitprice *  d.quantity) AS total_sales FROM order_details d 
INNER JOIN orders o ON o.orderid = d.orderid
INNER JOIN customers c ON o.customerid = c.customerid
GROUP BY o.customerid ,  c.companyname , c.contactname;

--Find top 10 products by total quantity sold
SELECT o.productid , p.productname , p.unitprice , SUM(o.quantity) AS total_quantity_sold FROM order_details o
INNER JOIN products p ON p.productid = o.productid
GROUP BY o.productid , p.productname , p.unitprice ORDER BY SUM(o.quantity) DESC LIMIT 10;

--Find categories whose total sales are greater than 50000
SELECT c.categoryname ,  ROUND(SUM(o.unitprice * o.quantity),2) AS total_sales FROM order_details o 
INNER JOIN products p ON p.productid = o.productid
INNER JOIN categories c ON p.categoryid = c.categoryid
GROUP BY c.categoryid HAVING SUM(o.unitprice * o.quantity) > 50000;

--Find employees whose total sales are greater than 100000
SELECT e.employeeid , e.lastname , e.firstname , e.title , SUM(d.unitprice *  d.quantity) AS total_sales FROM order_details d 
INNER JOIN orders o ON o.orderid = d.orderid
INNER JOIN employees e ON e.employeeid = o.employeeid
GROUP BY e.employeeid  , e.lastname , e.firstname , e.title HAVING SUM(d.unitprice *  d.quantity) > 100000;

--Find total sales per country based on customer country
SELECT c.country , SUM(d.unitprice *  d.quantity) AS total_sales FROM customers c
INNER JOIN orders o ON o.customerid = c.customerid
INNER JOIN order_details d ON d.orderid = o.orderid
GROUP BY c.country;

--Find suppliers whose products generated sales above 30000
SELECT s.supplierid , s.companyname , ROUND(SUM(d.unitprice *  d.quantity),2) AS total_sales FROM suppliers s
INNER JOIN products p ON s.supplierid = p.supplierid
INNER JOIN order_details d ON d.productid = p.productid
GROUP BY s.supplierid , s.companyname HAVING SUM(d.unitprice *  d.quantity) > 30000;

--Find customers who placed more than 10 orders and sort by order count descending
SELECT o.customerid , c.companyname , c.contactname , COUNT(o.customerid) AS total_orders FROM orders o 
INNER JOIN customers c ON c.customerid = o.customerid
GROUP BY o.customerid , c.companyname , c.contactname HAVING COUNT(o.customerid) > 10
ORDER BY COUNT(o.customerid) DESC;

--Find monthly order count for each year and month
SELECT date_part('year',orderdate) AS year , date_part('month',orderdate) AS month , COUNT(*) AS total_orders FROM orders
GROUP BY date_part('year',orderdate) , date_part('month',orderdate)
ORDER BY year , month;

--Find monthly sales amount ordered by year and month
SELECT date_part('year',o.orderdate) AS year , date_part('month',o.orderdate) AS month , ROUND(SUM(d.unitprice * d.quantity),2) AS total_sales FROM orders o
INNER JOIN order_details d ON o.orderid = d.orderid
GROUP BY date_part('year',o.orderdate) , date_part('month',o.orderdate)
ORDER BY year , month;