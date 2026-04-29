--BASIC TO INTERMEDIATE

--List all customers and the total number of orders they have placed. Show only customers with more than 5 orders. Sort by total orders descending
SELECT c.companyname , c.contactname , COUNT(o.orderid) AS total_orders FROM customers c 
INNER JOIN orders o ON c.customerid = o.customerid
GROUP BY o.customerid , c.companyname , c.contactname HAVING COUNT(o.orderid) > 5 ORDER BY total_orders DESC;

--Retrieve the total sales amount per customer by joining customers, orders, and order_details. Show only customers whose total sales exceed 10,000. Sort by total sales descending
SELECT c.companyname , c.contactname , ROUND(SUM(d.unitprice * d.quantity * (1 - d.discount))::NUMERIC,2) AS total_sales FROM customers c
INNER JOIN orders o ON c.customerid = o.customerid
INNER JOIN order_details d ON o.orderid = d.orderid
GROUP BY o.customerid , c.companyname , c.contactname 
HAVING SUM(d.unitprice * d.quantity * (1 - d.discount)) > 10000 ORDER BY total_sales DESC;

--Get the number of products per category. Show only categories having more than 10 products. Sort by product count descending
SELECT c.categoryname , COUNT(p.productid) AS total_products FROM categories c 
INNER JOIN products p ON p.categoryid = c.categoryid
GROUP BY p.categoryid , c.categoryname
HAVING COUNT(p.productid) > 10 ORDER BY total_products DESC;

--Display the total quantity sold per product.Include only products where total quantity sold is greater than 100. Sort by quantity descending.
SELECT o.productid , p.productname , p.unitprice , SUM(o.quantity) AS total_quantity_sold FROM order_details o
INNER JOIN products p ON p.productid = o.productid
GROUP BY o.productid , p.productname , p.unitprice HAVING SUM(o.quantity) > 100 ORDER BY total_quantity_sold DESC;

--Find the total number of orders handled by each employee.Show only employees who handled more than 20 orders. Sort by order count descending.
SELECT o.employeeid , e.lastname , e.firstname , e.title ,COUNT(o.orderid) AS total_orders FROM orders o
INNER JOIN employees e ON o.employeeid = e.employeeid
GROUP BY o.employeeid , e.lastname , e.firstname , e.title
HAVING COUNT(o.orderid) > 20 ORDER BY total_orders DESC;


--INTERMEDIATE

--Retrieve the total sales per category by joining categories, products, and order_details. Show only categories with total sales above 50,000. Sort by total sales descending.
SELECT c.categoryname ,  ROUND(SUM(o.unitprice * o.quantity * (1 - o.discount))::NUMERIC,2) AS total_sales FROM order_details o 
INNER JOIN products p ON p.productid = o.productid
INNER JOIN categories c ON p.categoryid = c.categoryid
GROUP BY c.categoryid , c.categoryname HAVING SUM(o.unitprice * o.quantity * (1 - o.discount)) > 50000 ORDER BY total_sales DESC;

--List suppliers and the number of products they supply. Show only suppliers who supply more than 5 products. Sort by product count descending.
SELECT s.supplierid , s.companyname , s.contactname , COUNT(p.productid) AS total_products FROM suppliers s
INNER JOIN products p ON s.supplierid = p.supplierid
GROUP BY s.supplierid , s.companyname , s.contactname HAVING COUNT(p.productid) > 5 ORDER BY total_products DESC;

--Get the average unit price per category. Show only categories where the average price is above 30. Sort by average price descending.
SELECT c.categoryname , ROUND(AVG(p.unitprice),2) AS average_product_price FROM categories c
INNER JOIN products p ON c.categoryid = p.categoryid
GROUP BY p.categoryid , c.categoryname HAVING AVG(p.unitprice) > 30 ORDER BY average_product_price DESC;

--Display the total revenue generated per employee (orders + order_details). Show only employees generating more than 20,000 in revenue. Sort by revenue descending.
SELECT e.employeeid , e.lastname , e.firstname , e.title , ROUND(SUM(d.unitprice *  d.quantity * (1 - d.discount))::NUMERIC,2) AS total_sales FROM order_details d 
INNER JOIN orders o ON o.orderid = d.orderid
INNER JOIN employees e ON e.employeeid = o.employeeid
GROUP BY e.employeeid  , e.lastname , e.firstname , e.title HAVING SUM(d.unitprice *  d.quantity * (1 - d.discount)) > 20000 ORDER BY total_Sales DESC;

--Retrieve the number of orders shipped to each country. Show only countries with more than 10 orders. Sort by order count descending.
SELECT shipcountry , COUNT(*) AS total_orders FROM orders GROUP BY shipcountry HAVING COUNT(*) > 10 ORDER BY total_orders DESC;


--ADVANCED

--Find customers and the average order value (orders + order_details). Show only customers with average order value greater than 500. Sort by average descending.
SELECT o.customerid , c.companyname , c.contactname , ROUND(AVG(d.unitprice *  d.quantity * (1 - d.discount))::NUMERIC,2) AS average_sales FROM order_details d 
INNER JOIN orders o ON o.orderid = d.orderid
INNER JOIN customers c ON o.customerid = c.customerid
GROUP BY o.customerid ,  c.companyname , c.contactname 
HAVING AVG(d.unitprice *  d.quantity * (1 - d.discount)) > 500 ORDER BY average_sales DESC;

--Get the top-selling products per category (by total quantity sold).Show only products with total quantity sold above 200. Sort within category by quantity descending.
SELECT DISTINCT ON (c.categoryid) c.categoryname, p.productname, SUM(o.quantity) AS total_quantity FROM categories c 
INNER JOIN products p ON c.categoryid = p.categoryid
INNER JOIN order_details o ON o.productid = p.productid
GROUP BY c.categoryid, c.categoryname, p.productid, p.productname
HAVING SUM(o.quantity) > 200 ORDER BY c.categoryid, total_quantity DESC;

--Retrieve the total discount given per product (order_details). Show only products where total discount exceeds 1,000. Sort by discount descending.
SELECT p.productname , p.unitprice  , ROUND(SUM(d.unitprice * d.quantity * d.discount)::NUMERIC,2) AS discount_given FROM products p 
INNER JOIN order_details d ON d.productid = p.productid
GROUP BY p.productid , p.productname , p.unitprice 
HAVING SUM(d.unitprice * d.quantity * d.discount) > 1000 ORDER BY discount_given DESC;

--List employees and the number of unique customers they handled. Show only employees who handled more than 15 unique customers. Sort by count descending.
SELECT o.employeeid , e.lastname , e.firstname , e.title , COUNT(DISTINCT(o.customerid)) AS customers_handled FROM orders o
INNER JOIN employees e ON o.employeeid = e.employeeid
GROUP BY o.employeeid ,e.lastname , e.firstname , e.title HAVING COUNT(DISTINCT(o.customerid)) > 15 ORDER BY customers_handled DESC;

--Find the monthly total sales (year + month) using orders and order_details.Show only months where total sales exceed 30,000. Sort by year and month ascending.
SELECT date_part('year',o.orderdate) AS year , date_part('month',o.orderdate) AS month , ROUND(SUM(d.unitprice * d.quantity * (1 - d.discount))::NUMERIC,2) AS total_sales FROM orders o
INNER JOIN order_details d ON o.orderid = d.orderid
GROUP BY date_part('year',o.orderdate) , date_part('month',o.orderdate) HAVING SUM(d.unitprice * d.quantity * (1 - d.discount)) > 30000
ORDER BY year , month;


--Transaction and SP

--Create a stored procedure to insert a new customer. Use a transaction so that if any required value is missing, the insert is rolled back.
CREATE OR REPLACE PROCEDURE proc_create_customer(i_customer_id VARCHAR(4) , i_companyname VARCHAR(40) , i_contactname VARCHAR(30) , i_contacttitle VARCHAR(30) , i_address VARCHAR(60) , i_city VARCHAR(15) , i_region VARCHAR(15) , i_postalcode VARCHAR(10) , i_country VARCHAR(12) , i_phone VARCHAR(24) , i_fax VARCHAR(24))
LANGUAGE plpgsql 
AS $$
BEGIN
    IF(i_companyname IS NULL OR i_contactname IS NULL OR i_phone IS NULL) THEN
        ROLLBACK;
    ELSE
        INSERT INTO customers(customerid , companyname, contactname, contacttitle, address, city, region, postalcode, country, phone, fax)
        VALUES(i_customer_id , i_companyname,i_contactname,i_contacttitle,i_address,i_city,i_region,i_postalcode,i_country,i_phone,i_fax);
        COMMIT;
    END IF;
END;
$$;

CALL proc_create_customer('A101','Aswin', 'Aswin', 'Engineer', 'Chennai Address', 'Chennai', 'india', '600001', 'India', '9876543210', 'demo');


--Create a stored procedure to place a new order for an existing customer with one product. Insert into orders and order_details in a single transaction
CREATE OR REPLACE PROCEDURE proc_create_order(i_productid INT , i_unitprice NUMERIC(19,4) , i_quantity SMALLINT , i_discount REAL , i_customer_id VARCHAR(5) , i_employeeid INT , i_orderdate TIMESTAMP , i_requireddate TIMESTAMP , i_shippeddate TIMESTAMP , i_shipvia INT , i_freight NUMERIC(19,4) , i_shipname VARCHAR(40) , i_shipaddress VARCHAR(60) , i_shipcity VARCHAR(15) , i_shipregion VARCHAR(15) , i_shippostalcode VARCHAR(10) , i_shipcountry VARCHAR(15))
LANGUAGE plpgsql
AS $$
DECLARE
    i_orderid INT;
BEGIN
    INSERT INTO orders(customerid  , employeeid , orderdate , requireddate , shippeddate, shipvia , freight , shipname , shipaddress , shipcity , shipregion , shippostalcode, shipcountry ) 
    VALUES(i_customer_id,i_employeeid,i_orderdate,i_requireddate,i_shippeddate,i_shipvia,i_freight,i_shipname,i_shipaddress,i_shipcity,i_shipregion,i_shippostalcode,i_shipcountry)
    RETURNING orderid INTO i_orderid;

    INSERT INTO order_details(orderid , productid , unitprice , quantity , discount) VALUES(i_orderid , i_productid , i_unitprice , i_quantity , i_discount);
    COMMIT;
END;
$$;

CALL proc_create_order(1, 10::NUMERIC, 5::SMALLINT, 0.1, 'A101'::VARCHAR, 1, NOW()::TIMESTAMP, (NOW() + INTERVAL '5 days')::TIMESTAMP, NULL::TIMESTAMP, 1, 50::NUMERIC, 'Aswin'::VARCHAR, 'Chennai Address'::VARCHAR, 'Chennai'::VARCHAR, 'TN'::VARCHAR, '600001'::VARCHAR, 'India'::VARCHAR);

--Create a stored procedure to update product stock after an order is placed. If stock is not enough, rollback the transaction.
CREATE OR REPLACE PROCEDURE proc_update_stock(i_productid INT , quantity INT)
LANGUAGE plpgsql
AS $$
DECLARE
    available_stock INT;
BEGIN
    SELECT unitsinstock INTO available_stock FROM products WHERE productid = i_productid;
    if((available_stock - quantity) >= 0) THEN
        UPDATE products SET unitsinstock = unitsinstock - quantity  WHERE productid = i_productid;
        COMMIT;
    ELSE
        ROLLBACK;
    END IF;
END;
$$;

CALL proc_update_stock(1,10);


--Create a stored procedure to cancel an order. Delete records from order_details first, then from orders, using a transaction
CREATE OR REPLACE PROCEDURE proc_delete_order(i_orderid INT)
LANGUAGE plpgsql
AS $$
DECLARE
    o_orderid INT;
    d_orderid INT;
BEGIN
    SELECT orderid INTO o_orderid FROM orders WHERE orderid = i_orderid;
    SELECT orderid INTO d_orderid FROM order_details WHERE orderid = i_orderid;

    IF(o_orderid IS NOT NULL AND d_orderid IS NOT NULL) THEN
        DELETE FROM order_details WHERE orderid = d_orderid;
        DELETE FROM orders WHERE orderid = o_orderid;
        COMMIT;
    ELSE
        ROLLBACK;
    END IF;
END;
$$;

CALL proc_delete_order(10248);

--Create a stored procedure to transfer products from one supplier to another. If the old supplier or new supplier does not exist, rollback.
CREATE OR REPLACE PROCEDURE proc_update_supplier(i_productid INT , new_supplierid INT)
LANGUAGE plpgsql
AS $$
DECLARE
    old_supplierid INT;
    v_new_supplierid INT; 
BEGIN
    SELECT supplierid INTO old_supplierid FROM products WHERE productid = i_productid;
    SELECT supplierid INTO v_new_supplierid FROM suppliers WHERE supplierid = new_supplierid;

    iF(old_supplierid IS NOT NULL AND v_new_supplierid IS NOT NULL) THEN
        UPDATE products SET supplierid = new_supplierid WHERE productid = i_productid;
        COMMIT;
    ELSE
        ROLLBACK;
    END IF;
END;
$$;

CALL proc_update_supplier(1,2);

--Create a stored procedure to update the price of all products in a category by a percentage. Rollback if the percentage is less than or equal to zero.
CREATE OR REPLACE PROCEDURE proc_update_price(i_categoryid INT , percentage INT)
LANGUAGE plpgsql
AS $$
BEGIN
    IF(percentage > 0) THEN
        UPDATE products SET unitprice = unitprice + ((unitprice / 100) * percentage) WHERE categoryid = i_categoryid;
        COMMIT;
    ELSE
        ROLLBACK;
    END IF;
END;
$$;

CALL proc_update_price(1,100);


--Create a stored procedure to add a new product under an existing category and supplier. Rollback if the category or supplier does not exist.
CREATE OR REPLACE PROCEDURE proc_insert_products(i_productname VARCHAR(40) , i_supplierid INT , i_categoryid INT , i_quantityperunit VARCHAR(20) , i_unitprice NUMERIC(19,4) , i_unitsinstock SMALLINT  , i_unitsonorder SMALLINT , i_reorderlevel SMALLINT , i_discontinued SMALLINT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_suppliedid INT;
    v_categoryid INT;
BEGIN
    SELECT supplierid INTO v_suppliedid FROM suppliers WHERE supplierid = i_supplierid;
    SELECT categoryid INTO v_categoryid FROM categories WHERE categoryid = i_categoryid;

    IF(v_suppliedid IS NOT NULL AND v_categoryid IS NOT NULL) THEN
        INSERT INTO products(productname , supplierid  , categoryid , quantityperunit ,  unitprice , unitsinstock , unitsonorder , reorderlevel , discontinued )
        VALUES(i_productname,i_supplierid , i_categoryid , i_quantityperunit , i_unitprice , i_unitsinstock,i_unitsonorder ,i_reorderlevel , i_discontinued);
        COMMIT;
    ELSE
        ROLLBACK;
    END IF;
END;
$$;

CALL proc_insert_products('APPLE',1,2,'DEMO',10,10,10,10,0);


--Create a stored procedure to delete a customer only if the customer has no orders. Use a transaction and rollback if orders exist.
CREATE OR REPLACE PROCEDURE proc_delete_customer(i_customer_id CHARACTER(5))
LANGUAGE plpgsql
AS $$
DECLARE
    total_order INT;
BEGIN   
    SELECT COUNT(*) INTO total_order FROM orders WHERE customerid = i_customer_id;
    iF(total_order > 0) THEN
        ROLLBACK;
    ELSE
        DELETE FROM customers WHERE customerid = i_customer_id;
        COMMIT;
    END IF;
END;
$$;

CALL proc_delete_customer('ALFKI');


--Create a stored procedure to apply a discount to all order details for a specific order. Rollback if the order does not exist.
CREATE OR REPLACE PROCEDURE proc_update_discount(i_orderid INT,i_discount REAL)
LANGUAGE plpgsql
AS $$
DECLARE
    total_order INT;
BEGIN
    SELECT COUNT(*) INTO total_order FROM order_details WHERE orderid = i_orderid;

    IF(total_order > 0) THEN
        UPDATE order_details SET discount = i_discount WHERE orderid = i_orderid;
        COMMIT;
    ELSE
        ROLLBACK;
    END IF;
END;
$$;

CALL proc_update_discount(10249,0.10);


--Create a stored procedure to place an order with multiple products. Insert the order and all order items in one transaction. If any product is invalid or stock is insufficient, rollback the complete order.
CREATE OR REPLACE PROCEDURE proc_create_order(i_productids INT[], i_unitprices NUMERIC(19,4)[], i_quantities SMALLINT[], i_discounts REAL[], i_customer_id VARCHAR(5), i_employeeid INT, i_orderdate TIMESTAMP, i_requireddate TIMESTAMP, i_shippeddate TIMESTAMP, i_shipvia INT, i_freight NUMERIC(19,4), i_shipname VARCHAR(40), i_shipaddress VARCHAR(60), i_shipcity VARCHAR(15), i_shipregion VARCHAR(15), i_shippostalcode VARCHAR(10), i_shipcountry VARCHAR(15))
LANGUAGE plpgsql
AS $$
DECLARE
    i_orderid INT;
    i INT;
BEGIN
    INSERT INTO orders(customerid, employeeid, orderdate, requireddate, shippeddate, shipvia, freight, shipname, shipaddress, shipcity, shipregion, shippostalcode, shipcountry)
    VALUES(i_customer_id, i_employeeid, i_orderdate, i_requireddate, i_shippeddate, i_shipvia, i_freight, i_shipname, i_shipaddress, i_shipcity, i_shipregion, i_shippostalcode, i_shipcountry)
    RETURNING orderid INTO i_orderid;

    FOR i IN 1..array_length(i_productids, 1) LOOP
    DECLARE
        v_productid INT;
        v_stock INT;
        BEGIN
            SELECT productid INTO v_productid FROM products WHERE productid = i_productids[i];
            SELECT unitsinstock INTO v_stock FROM products WHERE productid = i_productids[i];

            IF(v_productid IS NOT NULL AND v_stock >= i_quantities[i]) THEN
                INSERT INTO order_details(orderid, productid, unitprice, quantity, discount)
                VALUES(i_orderid, i_productids[i], i_unitprices[i], i_quantities[i], i_discounts[i]);

            ELSE
                ROLLBACK;
            END IF;
        END;
    END LOOP;

    COMMIT;
END;
$$;

CALL proc_create_order(ARRAY[1,2], ARRAY[10,20], ARRAY[5,3], ARRAY[0.1,0.05], 'A101'::VARCHAR, 1, NOW()::TIMESTAMP, NOW()::TIMESTAMP + INTERVAL '5 days', NULL::TIMESTAMP, 1, 50::NUMERIC, 'Aswin'::VARCHAR, 'Chennai Address'::VARCHAR, 'Chennai'::VARCHAR, 'TN'::VARCHAR, '600001'::VARCHAR, 'India'::VARCHAR);