CREATE OR REPLACE PROCEDURE borrow_book(i_member_id INT , i_bookcopy_id INT)
LANGUAGE plpgsql
AS $$
DECLARE
    membership_isactive BOOLEAN;
    member_limit INT;
    member_max_limit INT;
    member_borrow_days INT;
    book_copy_status VARCHAR(15);
    member_fine_amount NUMERIC(6,2);
    already_borrowed BOOLEAN;
    due_date TIMESTAMP;
BEGIN 
    --member is active or not and max limit of books borrowed and max days
    SELECT t.maxborrowlimit , t.maxborrowdays , m.isactive INTO member_max_limit , member_borrow_days , membership_isactive FROM membershiptype t INNER JOIN member m ON t.id = m.membershiptypeid where m.id = i_member_id;

    IF membership_isactive IS NULL THEN --check the user is present
        RAISE EXCEPTION 'MEMBER NOT PRESENT';
    END IF;

    --get book copy status
    SELECT bookstatus INTO book_copy_status FROM bookcopies WHERE id = i_bookcopy_id;

    IF book_copy_status IS NULL THEN --if book copy not present
        RAISE EXCEPTION 'BOOK COPY NOT PRESENT';
    END IF;

    --check number of book copies borrowed
    SELECT COUNT(*) INTO member_limit FROM borrowing WHERE memberid = i_member_id AND borrowstatus = 'BORROWED';

    --check overall fine 
    SELECT COALESCE(SUM(amount),0) INTO member_fine_amount FROM fine WHERE memberid = i_member_id AND ispaid = FALSE;

    --check already borrowed same book
    SELECT EXISTS (SELECT 1 FROM borrowing br JOIN bookcopies bc ON br.bookcopyid = bc.id WHERE br.memberid = i_member_id AND bc.bookid = (SELECT bookid FROM bookcopies WHERE id = i_bookcopy_id) AND br.borrowstatus = 'BORROWED') INTO already_borrowed;


    IF membership_isactive = FALSE THEN --check member is active
        RAISE EXCEPTION 'MEMBER IS NOT ACTIVE';
    END IF;

    IF member_fine_amount >= 500 THEN --check fine amount is greater than 500
        RAISE EXCEPTION 'MEMBER EXCEEDS FINE LIMIT ₹500';
    END IF;

    IF book_copy_status != 'AVAILABLE' THEN --check book copy is available
        RAISE EXCEPTION 'BOOK COPY IS NOT AVAILABLE';
    END IF;

    IF member_limit >= member_max_limit THEN --check max borrowing limit
        RAISE EXCEPTION 'MEMBER ALREADY REACHED THE BORROWING LIMIT';
    END IF;

    IF already_borrowed = TRUE THEN --already having the same book
        RAISE EXCEPTION 'MEMBER ALREADY BORROWED SAME BOOK';
    END IF;


    due_date := NOW() + (member_borrow_days || ' day')::INTERVAL;
    INSERT INTO borrowing(memberid , bookcopyid , borrowdate , duedate , borrowstatus) VALUES(i_member_id,i_bookcopy_id,NOW(),due_date,'BORROWED');

    UPDATE bookcopies SET bookstatus = 'BORROWED' WHERE id = i_bookcopy_id;

END;
$$;


CREATE OR REPLACE PROCEDURE return_book( i_borrowing_id INT, i_condition VARCHAR(10))
LANGUAGE plpgsql
AS $$
DECLARE
    v_memberid INT;
    v_bookcopyid INT;
    v_duedate TIMESTAMP;
    v_borrowstatus VARCHAR(15);
    v_late_days INT;
    v_fine_amount NUMERIC(6,2);
    v_fine_id INT;
BEGIN
    SELECT memberid, bookcopyid, duedate, borrowstatus INTO v_memberid , v_bookcopyid , v_duedate , v_borrowstatus FROM borrowing WHERE id = i_borrowing_id;

    IF v_memberid IS NULL THEN --check borrowing is present
        RAISE EXCEPTION 'BORROWING NOT FOUND';
    END IF;

    IF v_borrowstatus = 'RETURNED' THEN --check the book is not returned
        RAISE EXCEPTION 'BOOK COPY ALREADY RETURNED';
    END IF;

    v_late_days := GREATEST(0, EXTRACT(DAY FROM (NOW() - v_duedate)));--calculate the total fine days 

    IF v_late_days > 0 THEN --calculate the total fine
        v_fine_amount := v_late_days * 10;
        INSERT INTO FINE(borrowingid , memberid , amount , finereason) VALUES(i_borrowing_id,v_memberid,v_fine_amount,'LATE_RETURN');
    END IF;

    IF i_condition = 'DAMAGED' THEN --condition if book is damaged
        UPDATE bookcopies SET bookstatus = 'DAMAGED' WHERE id = v_bookcopyid;
        INSERT INTO fine(borrowingid , memberid , amount , finereason) VALUES(i_borrowing_id,v_memberid,200,'DAMAGE') RETURNING id INTO v_fine_id;
        INSERT INTO bookdamagereport(borrowingid,fineid,description) VALUES(i_borrowing_id,v_fine_id,'USER DAMAGED THE BORROWED BOOK');

    ELSIF i_condition = 'LOST' THEN --condition is book is lost
        UPDATE bookcopies SET bookstatus = 'DAMAGED' WHERE id = v_bookcopyid;
        INSERT INTO fine(borrowingid , memberid , amount , finereason) VALUES(i_borrowing_id,v_memberid,400,'LOST') RETURNING id INTO v_fine_id;
        INSERT INTO bookdamagereport(borrowingid,fineid,description) VALUES(i_borrowing_id,v_fine_id,'USER LOST THE BORROWED BOOK');

    ELSIF i_condition = 'NORMAL' THEN --condition if book is normal
        UPDATE bookcopies SET bookstatus = 'AVAILABLE' WHERE id = v_bookcopyid;

    ELSE --not a valid condition
        RAISE EXCEPTION 'INVALID CONDITION';
    END IF;

    UPDATE borrowing SET borrowstatus = 'RETURNED' , returndate = NOW() WHERE id = i_borrowing_id;
END;
$$;