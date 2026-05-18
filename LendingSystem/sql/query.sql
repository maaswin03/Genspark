--MembershipType table
CREATE TABLE membershiptype(
    id SERIAL CONSTRAINT pk_membershiptype_id PRIMARY KEY,
    typename VARCHAR(20) NOT NULL UNIQUE,
    maxborrowlimit INT NOT NULL,
    maxborrowdays INT NOT NULL
);

--Member table
CREATE TABLE member(
    id SERIAL CONSTRAINT pk_member_id PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    email VARCHAR(50) NOT NULL UNIQUE,
    phone VARCHAR(15) NOT NULL UNIQUE,
    isactive BOOLEAN NOT NULL DEFAULT TRUE,
    membershiptypeid INT NOT NULL,
    createdat TIMESTAMP NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_member_membershiptype_id FOREIGN KEY (membershiptypeid) REFERENCES membershiptype(id) 
);

--Book Category table
CREATE TABLE bookcategory(
    id SERIAL CONSTRAINT pk_bookcategory_id PRIMARY KEY,
    categoryname VARCHAR(50) NOT NULL UNIQUE
);

--Book table
CREATE TABLE book(
    id SERIAL CONSTRAINT pk_book_id PRIMARY KEY,
    title VARCHAR(100) NOT NULL,
    isbn VARCHAR(20) NOT NULL UNIQUE,
    author VARCHAR(50) NOT NULL,
    publishedyear INT NOT NULL CHECK(publishedyear > 1900),
    categoryid INT NOT NULL,

    CONSTRAINT fk_book_category_id FOREIGN KEY (categoryid) REFERENCES bookcategory(id) ON DELETE RESTRICT
);

--book copy enum
--CREATE TYPE book_copy_status AS ENUM('AVAILABLE','BORROWED','DAMAGED');

--Book copies table
CREATE TABLE bookcopies(
    id SERIAL CONSTRAINT pk_bookcopies_id PRIMARY KEY,
    bookid INT NOT NULL ,
    bookstatus VARCHAR(15) NOT NULL DEFAULT 'AVAILABLE' CHECK(bookstatus IN ('AVAILABLE','BORROWED','DAMAGED')),
    createdat TIMESTAMP NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_bookcopies_book_id FOREIGN KEY (bookid) REFERENCES book(id) ON DELETE RESTRICT
);

--borrow status
--CREATE TYPE borrow_status AS ENUM('BORROWED','RETURNED');

--Borrowing table
CREATE TABLE borrowing(
    id SERIAL CONSTRAINT pk_borrowing_id PRIMARY KEY,
    memberid INT NOT NULL,
    bookcopyid INT NOT NULL,
    borrowdate TIMESTAMP NOT NULL DEFAULT NOW(),
    duedate TIMESTAMP NOT NULL CHECK(duedate > borrowdate),
    returndate TIMESTAMP CHECK(returndate IS NULL OR returndate >= borrowdate),
    borrowstatus VARCHAR(10) NOT NULL DEFAULT 'BORROWED' CHECK(borrowstatus IN ('BORROWED','RETURNED')),

    CONSTRAINT fk_borrowing_member_id FOREIGN KEY (memberid) REFERENCES member(id) ON DELETE RESTRICT,
    CONSTRAINT fk_borrowing_bookcopy_id FOREIGN KEY (bookcopyid) REFERENCES bookcopies(id) ON DELETE RESTRICT
);

--fine reason 
-- CREATE TYPE fine_reason_type AS ENUM('LATE_RETURN','DAMAGE','LOST');

--fine table
CREATE TABLE fine(
    id SERIAL CONSTRAINT pk_fine_id PRIMARY KEY,
    borrowingid INT NOT NULL,
    memberid INT NOT NULL,
    amount NUMERIC(6,2) NOT NULL CHECK(amount > 0),
    finereason VARCHAR(20) NOT NULL CHECK(finereason IN ('LATE_RETURN','DAMAGE','LOST')),
    ispaid BOOLEAN NOT NULL DEFAULT FALSE,
    createdat TIMESTAMP NOT NULL DEFAULT NOW(),
    paidat TIMESTAMP,

    CONSTRAINT fk_fine_borrowing_id FOREIGN KEY (borrowingid) REFERENCES borrowing(id) ON DELETE RESTRICT,
    CONSTRAINT fk_fine_member_id FOREIGN KEY (memberid) REFERENCES member(id) ON DELETE RESTRICT
);

--payment method type
-- CREATE TYPE payment_method_type AS ENUM('CARD','UPI','NET_BANKING','WALLET');

--fine payment table
CREATE TABLE finepayment(
    id SERIAL CONSTRAINT pk_finepayment_id PRIMARY KEY,
    fineid INT NOT NULL,
    amountpaid NUMERIC(6,2) NOT NULL CHECK(amountpaid > 0),
    paymentmethod VARCHAR(10) NOT NULL CHECK(paymentmethod IN ('CARD','UPI','NET_BANKING','WALLET')),
    createdat TIMESTAMP NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_finepayment_fine_id FOREIGN KEY (fineid) REFERENCES fine(id) ON DELETE RESTRICT
);

--damage report table
CREATE TABLE bookdamagereport(
    id SERIAL CONSTRAINT pk_bookdamagereport_id PRIMARY KEY,
    borrowingid INT NOT NULL,
    fineid INT,
    description TEXT NOT NULL,
    reportedat TIMESTAMP NOT NULL DEFAULT NOW(),
    isresolved BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT fk_bookdamagereport_borrowing_id FOREIGN KEY (borrowingid) REFERENCES borrowing(id) ON DELETE RESTRICT,
    CONSTRAINT fk_bookdamagereport_fine_id FOREIGN KEY (fineid) REFERENCES fine(id) ON DELETE RESTRICT
);


ALTER TABLE finepayment ADD COLUMN paymentstatus VARCHAR(50) NOT NULL DEFAULT 'SUCCESS' CHECK(paymentstatus IN ('SUCCESS','CANCELLED','REFUNDED'));
ALTER TABLE member ADD COLUMN password VARCHAR(50) NOT NULL;
ALTER TABLE member ADD COLUMN password_set BOOLEAN NOT NULL DEFAULT FALSE;