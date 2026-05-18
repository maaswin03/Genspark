--function for retuning the calculate_member_fine
CREATE OR REPLACE FUNCTION calculate_member_fine(i_memberid INT)
RETURNS NUMERIC(6,2)
AS $$
DECLARE
    total_sum NUMERIC(6,2);
BEGIN
    SELECT SUM(amount) INTO total_sum FROM fine WHERE memberid =i_memberid AND ispaid = FALSE;

    IF total_sum IS NULL THEN
        RETURN 0;
    END IF;

    RETURN total_sum;
END;
$$ LANGUAGE plpgsql;