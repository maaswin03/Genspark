-- ============================================================
-- PROJECT : BusManagementSystem
-- DATABASE : PostgreSQL 14+
-- VERSION  : 3.0  (Production-Ready)
-- AUTHOR   : Generated from DBML v3.0
--
-- HOW TO RUN:
--   psql -U <user> -d <database> -f bus_management_schema.sql
--
-- NOTES:
--   - Run this file once on a fresh database.
--   - All ENUMs are created first, then tables in FK-safe order.
--   - All CHECK constraints are real DB-level constraints.
--   - Indexes are created after table definitions.
--   - A seed block at the bottom inserts the three roles.
--   - Wrap in a transaction so the whole file is atomic.
-- ============================================================

BEGIN;

-- ============================================================
-- 0. EXTENSIONS
-- ============================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";   -- for gen_random_uuid() if needed later


-- ============================================================
-- 1. ENUM TYPES
-- ============================================================

CREATE TYPE approval_status_enum AS ENUM (
    'pending',    -- Awaiting admin review
    'approved',   -- Operator is active and can operate
    'rejected',   -- Application rejected — rejection_reason must be filled
    'blocked'     -- Previously approved but now blocked — blocked_reason must be filled
);

CREATE TYPE trip_status_enum AS ENUM (
    'scheduled',  -- Trip is upcoming, not yet departed
    'active',     -- Trip is currently in progress
    'completed',  -- Trip has arrived at destination
    'cancelled'   -- Trip was cancelled — cancellation_reason must be filled
);

CREATE TYPE booking_status_enum AS ENUM (
    'pending',    -- Payment not yet completed
    'confirmed',  -- Payment successful, booking is active
    'cancelled'   -- Booking was fully or partially cancelled
);

CREATE TYPE seat_status_enum AS ENUM (
    'available',  -- Seat is free to be reserved
    'reserved',   -- Temporarily locked — held for up to 5 minutes
    'booked'      -- Confirmed — payment completed
);

CREATE TYPE payment_status_enum AS ENUM (
    'pending',    -- Payment initiated but not yet confirmed by gateway
    'success',    -- Payment confirmed by gateway
    'failed'      -- Payment failed or declined
);

CREATE TYPE bus_type_enum AS ENUM (
    'AC_Sleeper',           -- Air-conditioned sleeper with upper/lower berths
    'Non_AC_Sleeper',       -- Non air-conditioned sleeper with upper/lower berths
    'AC_Seater',            -- Air-conditioned seater bus
    'Non_AC_Seater',        -- Non air-conditioned seater bus
    'AC_Semi_Sleeper',      -- Air-conditioned semi-sleeper (reclining seats)
    'Non_AC_Semi_Sleeper'   -- Non air-conditioned semi-sleeper
);

CREATE TYPE deck_type_enum AS ENUM (
    'lower',   -- Lower berth / ground floor of double-decker
    'upper',   -- Upper berth / top floor of double-decker
    'single'   -- Single-deck seater bus — no upper/lower distinction
);

CREATE TYPE seat_type_enum AS ENUM (
    'window',  -- Seat is next to window
    'aisle',   -- Seat is next to aisle
    'middle'   -- Seat is in the middle row (3-across layout)
);

CREATE TYPE gender_enum AS ENUM (
    'male',
    'female',
    'other'
);

CREATE TYPE notification_channel_enum AS ENUM (
    'email',
    'sms',
    'push',
    'in_app'
);

CREATE TYPE document_type_enum AS ENUM (
    'license',       -- Vehicle or operator license
    'registration',  -- Bus registration certificate
    'insurance',     -- Insurance document
    'other'
);

CREATE TYPE fee_type_enum AS ENUM (
    'percentage'     -- Platform fee is always a percentage of total booking amount
);

CREATE TYPE change_type_enum AS ENUM (
    'temporary',   -- Bus changed temporarily — reverted_at will be set when original bus is restored
    'permanent'    -- Bus permanently replaced — old bus is retired
);

CREATE TYPE booking_seat_status_enum AS ENUM (
    'confirmed',   -- Seat booking is active
    'cancelled'    -- Seat was individually cancelled (partial cancellation)
);

CREATE TYPE entity_type_enum AS ENUM (
    'operator',
    'booking',
    'trip',
    'bus',
    'route',
    'user',
    'seat',
    'schedule'
);

CREATE TYPE reference_type_enum AS ENUM (
    'booking',
    'trip',
    'operator',
    'bus'
);


-- ============================================================
-- 2. TABLES  (ordered to satisfy FK dependencies)
-- ============================================================

-- ------------------------------------------------------------
-- 2.01  roles
-- ------------------------------------------------------------
CREATE TABLE roles (
    id         SERIAL       PRIMARY KEY,
    name       VARCHAR(50)  NOT NULL,
    created_at TIMESTAMP    NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_roles_name UNIQUE (name)
);

COMMENT ON TABLE  roles      IS 'Static seed table. Must contain exactly: admin, operator, user';
COMMENT ON COLUMN roles.name IS 'Allowed values: admin, operator, user';


-- ------------------------------------------------------------
-- 2.02  users
-- ------------------------------------------------------------
CREATE TABLE users (
    id           SERIAL       PRIMARY KEY,
    name         VARCHAR(100) NOT NULL,
    email        VARCHAR(255) NOT NULL,
    password     VARCHAR(255) NOT NULL,   -- bcrypt hash — never plaintext
    phone        VARCHAR(20)  NOT NULL,
    role_id      INT          NOT NULL,
    is_active    BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at   TIMESTAMP    NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMP    NOT NULL DEFAULT NOW(),
    deleted_at   TIMESTAMP,               -- soft delete — NULL means active

    CONSTRAINT uq_users_email   UNIQUE  (email),
    CONSTRAINT fk_users_role_id FOREIGN KEY (role_id) REFERENCES roles (id)
);

COMMENT ON COLUMN users.password   IS 'Stored as bcrypt hash — never plaintext';
COMMENT ON COLUMN users.deleted_at IS 'Soft delete — NULL means active';


-- ------------------------------------------------------------
-- 2.03  operators
-- ------------------------------------------------------------
CREATE TABLE operators (
    id               SERIAL               PRIMARY KEY,
    user_id          INT                  NOT NULL,
    company_name     VARCHAR(200)         NOT NULL,
    license_number   VARCHAR(100)         NOT NULL,
    approval_status  approval_status_enum NOT NULL DEFAULT 'pending',
    approved_by      INT,                  -- FK -> users.id (admin)
    approved_at      TIMESTAMP,            -- set when approval_status = approved
    rejection_reason TEXT,                 -- required when approval_status = rejected
    blocked_reason   TEXT,                 -- required when approval_status = blocked
    created_at       TIMESTAMP            NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMP            NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_operators_user_id    UNIQUE  (user_id),
    CONSTRAINT fk_operators_user_id    FOREIGN KEY (user_id)    REFERENCES users (id),
    CONSTRAINT fk_operators_approved_by FOREIGN KEY (approved_by) REFERENCES users (id),

    -- When rejected, a reason must exist
    CONSTRAINT chk_operators_rejection_reason
        CHECK (approval_status <> 'rejected' OR rejection_reason IS NOT NULL),

    -- When blocked, a reason must exist
    CONSTRAINT chk_operators_blocked_reason
        CHECK (approval_status <> 'blocked'  OR blocked_reason   IS NOT NULL),

    -- When approved, approved_by and approved_at must be set
    CONSTRAINT chk_operators_approved_by_set
        CHECK (approval_status <> 'approved' OR (approved_by IS NOT NULL AND approved_at IS NOT NULL))
);

COMMENT ON TABLE operators IS
    'Business rules: rejection_reason required when rejected; blocked_reason required when blocked; '
    'approved_by+approved_at required when approved; approved_by must be an admin (app-level).';


-- ------------------------------------------------------------
-- 2.04  operator_documents
-- ------------------------------------------------------------
CREATE TABLE operator_documents (
    id            SERIAL              PRIMARY KEY,
    operator_id   INT                 NOT NULL,
    document_type document_type_enum  NOT NULL,
    file_url      VARCHAR(500)        NOT NULL,
    uploaded_at   TIMESTAMP           NOT NULL DEFAULT NOW(),
    verified_by   INT,                 -- FK -> users.id (admin who verified)
    verified_at   TIMESTAMP,           -- set when verified_by is assigned

    CONSTRAINT fk_op_docs_operator  FOREIGN KEY (operator_id) REFERENCES operators (id),
    CONSTRAINT fk_op_docs_verified  FOREIGN KEY (verified_by)  REFERENCES users (id),

    -- If verified_by is set, verified_at must also be set and vice versa
    CONSTRAINT chk_op_docs_verified_pair
        CHECK (
            (verified_by IS NULL AND verified_at IS NULL) OR
            (verified_by IS NOT NULL AND verified_at IS NOT NULL)
        )
);

COMMENT ON TABLE operator_documents IS 'verified_by must reference a user with role = admin (enforced at application level)';


-- ------------------------------------------------------------
-- 2.05  platform_fee_config
-- ------------------------------------------------------------
CREATE TABLE platform_fee_config (
    id           SERIAL        PRIMARY KEY,
    fee_type     fee_type_enum NOT NULL DEFAULT 'percentage',
    fee_value    NUMERIC(5,2)  NOT NULL,
    is_active    BOOLEAN       NOT NULL DEFAULT TRUE,
    created_by   INT           NOT NULL,
    created_at   TIMESTAMP     NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMP     NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_platform_fee_created_by FOREIGN KEY (created_by) REFERENCES users (id),

    -- Fee value must be a valid percentage
    CONSTRAINT chk_platform_fee_value
        CHECK (fee_value >= 0.00 AND fee_value <= 100.00)
);

COMMENT ON TABLE  platform_fee_config          IS 'Only one row with is_active=true should exist at a time (enforced at application level). created_by must be an admin.';
COMMENT ON COLUMN platform_fee_config.fee_value IS 'Percentage value e.g. 10.00 = 10%. Must be between 0.00 and 100.00';


-- ------------------------------------------------------------
-- 2.06  locations
-- ------------------------------------------------------------
CREATE TABLE locations (
    id         SERIAL       PRIMARY KEY,
    name       VARCHAR(100) NOT NULL,
    city       VARCHAR(100) NOT NULL,
    state      VARCHAR(100) NOT NULL,
    created_by INT          NOT NULL,
    created_at TIMESTAMP    NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_locations_name_city_state UNIQUE (name, city, state),
    CONSTRAINT fk_locations_created_by      FOREIGN KEY (created_by) REFERENCES users (id)
);

COMMENT ON TABLE locations IS 'Admin-managed only. created_by must reference an admin user (enforced at application level).';


-- ------------------------------------------------------------
-- 2.07  routes
-- ------------------------------------------------------------
CREATE TABLE routes (
    id             SERIAL    PRIMARY KEY,
    source_id      INT       NOT NULL,
    destination_id INT       NOT NULL,
    distance_km    INT       NOT NULL,
    is_active      BOOLEAN   NOT NULL DEFAULT TRUE,
    created_by     INT       NOT NULL,
    created_at     TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at     TIMESTAMP NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_routes_source_dest        UNIQUE (source_id, destination_id),
    CONSTRAINT fk_routes_source             FOREIGN KEY (source_id)      REFERENCES locations (id),
    CONSTRAINT fk_routes_destination        FOREIGN KEY (destination_id) REFERENCES locations (id),
    CONSTRAINT fk_routes_created_by         FOREIGN KEY (created_by)     REFERENCES users (id),

    -- Source and destination must differ
    CONSTRAINT chk_routes_no_self_loop
        CHECK (source_id <> destination_id),

    -- Distance must be positive
    CONSTRAINT chk_routes_distance_positive
        CHECK (distance_km > 0)
);

COMMENT ON TABLE routes IS 'Admin-managed only. source_id != destination_id and distance_km > 0 enforced at DB level.';


-- ------------------------------------------------------------
-- 2.08  route_stops
-- ------------------------------------------------------------
CREATE TABLE route_stops (
    id                   SERIAL PRIMARY KEY,
    route_id             INT    NOT NULL,
    location_id          INT    NOT NULL,
    stop_order           INT    NOT NULL,
    distance_from_source INT    NOT NULL,

    CONSTRAINT uq_route_stops_route_order    UNIQUE (route_id, stop_order),
    CONSTRAINT uq_route_stops_route_location UNIQUE (route_id, location_id),
    CONSTRAINT fk_route_stops_route          FOREIGN KEY (route_id)    REFERENCES routes    (id),
    CONSTRAINT fk_route_stops_location       FOREIGN KEY (location_id) REFERENCES locations (id),

    CONSTRAINT chk_route_stops_order_positive
        CHECK (stop_order >= 1),

    CONSTRAINT chk_route_stops_distance_non_negative
        CHECK (distance_from_source >= 0)
);

COMMENT ON TABLE route_stops IS 'stop_order=1 must match routes.source_id; last stop must match routes.destination_id (enforced at application level).';


-- ------------------------------------------------------------
-- 2.09  operator_offices
-- ------------------------------------------------------------
CREATE TABLE operator_offices (
    id             SERIAL       PRIMARY KEY,
    operator_id    INT          NOT NULL,
    location_id    INT          NOT NULL,
    address        VARCHAR(500) NOT NULL,
    is_head_office BOOLEAN      NOT NULL DEFAULT FALSE,
    created_at     TIMESTAMP    NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_op_offices_operator FOREIGN KEY (operator_id) REFERENCES operators (id),
    CONSTRAINT fk_op_offices_location FOREIGN KEY (location_id) REFERENCES locations (id)
);

COMMENT ON TABLE operator_offices IS 'Each operator should have at most one head office (is_head_office=true). Enforced at application level.';


-- ------------------------------------------------------------
-- 2.10  boarding_points
-- ------------------------------------------------------------
CREATE TABLE boarding_points (
    id          SERIAL       PRIMARY KEY,
    route_id    INT          NOT NULL,
    location_id INT          NOT NULL,
    office_id   INT,                     -- optional link to operator office
    name        VARCHAR(200) NOT NULL,
    time_offset INT          NOT NULL,   -- minutes after trip departure_time
    is_default  BOOLEAN      NOT NULL DEFAULT FALSE,

    CONSTRAINT uq_boarding_route_loc_name   UNIQUE (route_id, location_id, name),
    CONSTRAINT fk_boarding_route            FOREIGN KEY (route_id)    REFERENCES routes           (id),
    CONSTRAINT fk_boarding_location         FOREIGN KEY (location_id) REFERENCES locations        (id),
    CONSTRAINT fk_boarding_office           FOREIGN KEY (office_id)   REFERENCES operator_offices (id),

    CONSTRAINT chk_boarding_time_offset_non_negative
        CHECK (time_offset >= 0)
);


-- ------------------------------------------------------------
-- 2.11  dropping_points
-- ------------------------------------------------------------
CREATE TABLE dropping_points (
    id          SERIAL       PRIMARY KEY,
    route_id    INT          NOT NULL,
    location_id INT          NOT NULL,
    office_id   INT,                     -- optional link to operator office
    name        VARCHAR(200) NOT NULL,
    time_offset INT          NOT NULL,   -- minutes after trip departure_time
    is_default  BOOLEAN      NOT NULL DEFAULT FALSE,

    CONSTRAINT uq_dropping_route_loc_name   UNIQUE (route_id, location_id, name),
    CONSTRAINT fk_dropping_route            FOREIGN KEY (route_id)    REFERENCES routes           (id),
    CONSTRAINT fk_dropping_location         FOREIGN KEY (location_id) REFERENCES locations        (id),
    CONSTRAINT fk_dropping_office           FOREIGN KEY (office_id)   REFERENCES operator_offices (id),

    CONSTRAINT chk_dropping_time_offset_non_negative
        CHECK (time_offset >= 0)
);


-- ------------------------------------------------------------
-- 2.12  buses
-- ------------------------------------------------------------
CREATE TABLE buses (
    id           SERIAL         PRIMARY KEY,
    operator_id  INT            NOT NULL,
    bus_number   VARCHAR(50)    NOT NULL,
    bus_type     bus_type_enum  NOT NULL,
    total_seats  INT            NOT NULL,
    is_active    BOOLEAN        NOT NULL DEFAULT TRUE,
    created_at   TIMESTAMP      NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMP      NOT NULL DEFAULT NOW(),
    deleted_at   TIMESTAMP,               -- soft delete — NULL means active

    CONSTRAINT uq_buses_bus_number      UNIQUE (bus_number),
    CONSTRAINT fk_buses_operator        FOREIGN KEY (operator_id) REFERENCES operators (id),

    CONSTRAINT chk_buses_total_seats_positive
        CHECK (total_seats > 0)
);

COMMENT ON COLUMN buses.deleted_at  IS 'Soft delete — NULL means active. A deleted bus must not be assigned to future trips.';
COMMENT ON COLUMN buses.total_seats IS 'Must be > 0. Should match COUNT(*) of active seats for this bus_id (enforced at application level).';


-- ------------------------------------------------------------
-- 2.13  seats
-- ------------------------------------------------------------
CREATE TABLE seats (
    id          SERIAL          PRIMARY KEY,
    bus_id      INT             NOT NULL,
    seat_number VARCHAR(10)     NOT NULL,   -- e.g. L1, L2, U1, U2
    row         INT             NOT NULL,
    "column"    INT             NOT NULL,
    deck        deck_type_enum  NOT NULL,
    seat_type   seat_type_enum  NOT NULL,
    is_active   BOOLEAN         NOT NULL DEFAULT TRUE,

    CONSTRAINT uq_seats_bus_seat_number UNIQUE (bus_id, seat_number),
    CONSTRAINT fk_seats_bus             FOREIGN KEY (bus_id) REFERENCES buses (id),

    CONSTRAINT chk_seats_row_positive
        CHECK (row >= 1),

    CONSTRAINT chk_seats_column_positive
        CHECK ("column" >= 1)
);

COMMENT ON TABLE seats IS 'deck=single for seater-only buses; deck=upper/lower for sleeper buses. Enforced at application level.';


-- ------------------------------------------------------------
-- 2.14  trip_schedules
-- ------------------------------------------------------------
CREATE TABLE trip_schedules (
    id             SERIAL         PRIMARY KEY,
    bus_id         INT            NOT NULL,
    route_id       INT            NOT NULL,
    operator_id    INT            NOT NULL,   -- denormalized for query performance
    departure_time TIME           NOT NULL,   -- time of day e.g. 21:00:00
    arrival_time   TIME           NOT NULL,   -- time of day
    base_fare      NUMERIC(10,2)  NOT NULL,
    days_of_week   VARCHAR(20)    NOT NULL,   -- e.g. '1,2,3,4,5,6,7' (Mon=1)
    valid_from     DATE           NOT NULL,
    valid_until    DATE,                       -- NULL = no end date
    is_active      BOOLEAN        NOT NULL DEFAULT TRUE,
    created_at     TIMESTAMP      NOT NULL DEFAULT NOW(),
    updated_at     TIMESTAMP      NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_trip_schedules_bus_route_dep UNIQUE (bus_id, route_id, departure_time),
    CONSTRAINT fk_trip_schedules_bus           FOREIGN KEY (bus_id)      REFERENCES buses     (id),
    CONSTRAINT fk_trip_schedules_route         FOREIGN KEY (route_id)    REFERENCES routes    (id),
    CONSTRAINT fk_trip_schedules_operator      FOREIGN KEY (operator_id) REFERENCES operators (id),

    CONSTRAINT chk_trip_schedules_base_fare_positive
        CHECK (base_fare > 0),

    -- valid_until must be on or after valid_from
    CONSTRAINT chk_trip_schedules_valid_dates
        CHECK (valid_until IS NULL OR valid_until >= valid_from)
);

COMMENT ON TABLE  trip_schedules             IS 'A cron job generates one trips row per day per active schedule.';
COMMENT ON COLUMN trip_schedules.days_of_week IS 'Comma-separated day numbers 1-7 where 1=Monday. e.g. "1,2,3,4,5" for weekdays. Validated at application level.';


-- ------------------------------------------------------------
-- 2.15  trips
-- ------------------------------------------------------------
CREATE TABLE trips (
    id                  SERIAL           PRIMARY KEY,
    schedule_id         INT,                          -- NULL for ad-hoc trips
    bus_id              INT              NOT NULL,    -- current assigned bus
    route_id            INT              NOT NULL,
    operator_id         INT              NOT NULL,    -- denormalized for query performance
    departure_time      TIMESTAMP        NOT NULL,
    arrival_time        TIMESTAMP        NOT NULL,
    base_fare           NUMERIC(10,2)    NOT NULL,
    status              trip_status_enum NOT NULL DEFAULT 'scheduled',
    cancellation_reason TEXT,                         -- required when status = cancelled
    created_at          TIMESTAMP        NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMP        NOT NULL DEFAULT NOW(),
    deleted_at          TIMESTAMP,                    -- soft delete

    CONSTRAINT fk_trips_schedule    FOREIGN KEY (schedule_id)  REFERENCES trip_schedules (id),
    CONSTRAINT fk_trips_bus         FOREIGN KEY (bus_id)        REFERENCES buses          (id),
    CONSTRAINT fk_trips_route       FOREIGN KEY (route_id)      REFERENCES routes         (id),
    CONSTRAINT fk_trips_operator    FOREIGN KEY (operator_id)   REFERENCES operators      (id),

    -- arrival must be after departure
    CONSTRAINT chk_trips_arrival_after_departure
        CHECK (arrival_time > departure_time),

    -- base fare must be positive
    CONSTRAINT chk_trips_base_fare_positive
        CHECK (base_fare > 0),

    -- cancellation_reason required when cancelled
    CONSTRAINT chk_trips_cancellation_reason
        CHECK (status <> 'cancelled' OR cancellation_reason IS NOT NULL)
);

COMMENT ON COLUMN trips.deleted_at IS 'Soft delete — a deleted trip must not accept new bookings (enforced at application level).';


-- ------------------------------------------------------------
-- 2.16  trip_seats
-- ------------------------------------------------------------
CREATE TABLE trip_seats (
    id           SERIAL           PRIMARY KEY,
    trip_id      INT              NOT NULL,
    seat_id      INT              NOT NULL,
    status       seat_status_enum NOT NULL DEFAULT 'available',
    locked_until TIMESTAMP,                -- set to now()+5min on reserve; NULL when booked/released
    locked_by    INT,                      -- FK -> users.id holding the 5-min lock

    CONSTRAINT uq_trip_seats_trip_seat   UNIQUE (trip_id, seat_id),   -- CRITICAL: prevents double booking
    CONSTRAINT fk_trip_seats_trip        FOREIGN KEY (trip_id)    REFERENCES trips (id),
    CONSTRAINT fk_trip_seats_seat        FOREIGN KEY (seat_id)    REFERENCES seats (id),
    CONSTRAINT fk_trip_seats_locked_by   FOREIGN KEY (locked_by)  REFERENCES users (id),

    -- locked_until and locked_by must both be set or both be NULL
    CONSTRAINT chk_trip_seats_lock_pair
        CHECK (
            (locked_until IS NULL AND locked_by IS NULL) OR
            (locked_until IS NOT NULL AND locked_by IS NOT NULL)
        ),

    -- locked_until only makes sense when status = reserved
    CONSTRAINT chk_trip_seats_lock_only_when_reserved
        CHECK (locked_until IS NULL OR status = 'reserved')
);

COMMENT ON TABLE trip_seats IS
    'Seat state machine: available->reserved(5min lock)->booked(payment ok). '
    'A pg_cron job must release expired reservations every minute: '
    'UPDATE trip_seats SET status=''available'', locked_until=NULL, locked_by=NULL '
    'WHERE status=''reserved'' AND locked_until < now();';


-- ------------------------------------------------------------
-- 2.17  seat_pricing
-- ------------------------------------------------------------
CREATE TABLE seat_pricing (
    id         SERIAL         PRIMARY KEY,
    trip_id    INT            NOT NULL,
    seat_id    INT            NOT NULL,
    price      NUMERIC(10,2)  NOT NULL,
    created_at TIMESTAMP      NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP      NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_seat_pricing_trip_seat UNIQUE (trip_id, seat_id),
    CONSTRAINT fk_seat_pricing_trip      FOREIGN KEY (trip_id) REFERENCES trips (id),
    CONSTRAINT fk_seat_pricing_seat      FOREIGN KEY (seat_id) REFERENCES seats (id),

    CONSTRAINT chk_seat_pricing_price_positive
        CHECK (price > 0)
);

COMMENT ON TABLE seat_pricing IS
    'Overrides trips.base_fare for a specific seat. '
    'Price is snapshotted into booking_seats.amount_paid at booking time — changes here do NOT affect confirmed bookings.';


-- ------------------------------------------------------------
-- 2.18  bookings
-- ------------------------------------------------------------
CREATE TABLE bookings (
    id                INT                 GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id           INT                 NOT NULL,
    trip_id           INT                 NOT NULL,
    boarding_point_id INT                 NOT NULL,
    dropping_point_id INT                 NOT NULL,
    booking_date      TIMESTAMP           NOT NULL DEFAULT NOW(),
    total_amount      NUMERIC(10,2)       NOT NULL,
    platform_fee      NUMERIC(10,2)       NOT NULL DEFAULT 0,
    status            booking_status_enum NOT NULL DEFAULT 'pending',
    cancelled_at      TIMESTAMP,           -- set when status = cancelled
    cancel_reason     TEXT,                -- required when status = cancelled
    created_at        TIMESTAMP           NOT NULL DEFAULT NOW(),
    updated_at        TIMESTAMP           NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_bookings_user             FOREIGN KEY (user_id)           REFERENCES users           (id),
    CONSTRAINT fk_bookings_trip             FOREIGN KEY (trip_id)            REFERENCES trips           (id),
    CONSTRAINT fk_bookings_boarding_point   FOREIGN KEY (boarding_point_id)  REFERENCES boarding_points (id),
    CONSTRAINT fk_bookings_dropping_point   FOREIGN KEY (dropping_point_id)  REFERENCES dropping_points (id),

    CONSTRAINT chk_bookings_total_amount_non_negative
        CHECK (total_amount >= 0),

    CONSTRAINT chk_bookings_platform_fee_non_negative
        CHECK (platform_fee >= 0),

    -- When cancelled, both cancelled_at and cancel_reason must be present
    CONSTRAINT chk_bookings_cancel_fields
        CHECK (
            status <> 'cancelled' OR
            (cancelled_at IS NOT NULL AND cancel_reason IS NOT NULL)
        )
);


-- ------------------------------------------------------------
-- 2.19  booking_seats
-- ------------------------------------------------------------
CREATE TABLE booking_seats (
    id           INT                      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    booking_id   INT                      NOT NULL,
    trip_seat_id INT                      NOT NULL,
    seat_id      INT                      NOT NULL,   -- denormalized for layout queries
    amount_paid  NUMERIC(10,2)            NOT NULL,
    status       booking_seat_status_enum NOT NULL DEFAULT 'confirmed',
    cancelled_at TIMESTAMP,                            -- set when status = cancelled

    CONSTRAINT uq_booking_seats_trip_seat   UNIQUE (trip_seat_id),   -- one booking per trip_seat
    CONSTRAINT fk_booking_seats_booking     FOREIGN KEY (booking_id)   REFERENCES bookings    (id),
    CONSTRAINT fk_booking_seats_trip_seat   FOREIGN KEY (trip_seat_id) REFERENCES trip_seats  (id),
    CONSTRAINT fk_booking_seats_seat        FOREIGN KEY (seat_id)      REFERENCES seats       (id),

    CONSTRAINT chk_booking_seats_amount_non_negative
        CHECK (amount_paid >= 0),

    -- When cancelled, cancelled_at must be set
    CONSTRAINT chk_booking_seats_cancel_fields
        CHECK (status <> 'cancelled' OR cancelled_at IS NOT NULL)
);

COMMENT ON TABLE booking_seats IS
    'When status->cancelled: release trip_seat, notify user, recalculate bookings.total_amount. '
    'If all seats cancelled, set bookings.status=cancelled. No monetary refund — notification only.';


-- ------------------------------------------------------------
-- 2.20  passengers
-- ------------------------------------------------------------
CREATE TABLE passengers (
    id              SERIAL       PRIMARY KEY,
    booking_seat_id INT          NOT NULL,
    name            VARCHAR(100) NOT NULL,
    age             INT          NOT NULL,
    gender          gender_enum  NOT NULL,

    CONSTRAINT uq_passengers_booking_seat   UNIQUE (booking_seat_id),  -- one passenger per seat per booking
    CONSTRAINT fk_passengers_booking_seat   FOREIGN KEY (booking_seat_id) REFERENCES booking_seats (id),

    CONSTRAINT chk_passengers_age_range
        CHECK (age >= 1 AND age <= 120)
);


-- ------------------------------------------------------------
-- 2.21  payments
-- ------------------------------------------------------------
CREATE TABLE payments (
    id               SERIAL               PRIMARY KEY,
    booking_id       INT                  NOT NULL,
    amount           NUMERIC(10,2)        NOT NULL,
    payment_status   payment_status_enum  NOT NULL DEFAULT 'pending',
    payment_method   VARCHAR(50)          NOT NULL,
    transaction_id   VARCHAR(255),                   -- gateway reference; NULL until attempted
    gateway_response JSONB,                           -- raw gateway response payload
    paid_at          TIMESTAMP,                       -- set when payment_status = success
    created_at       TIMESTAMP            NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMP            NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_payments_transaction_id       UNIQUE (transaction_id),
    CONSTRAINT fk_payments_booking              FOREIGN KEY (booking_id) REFERENCES bookings (id),

    CONSTRAINT chk_payments_amount_positive
        CHECK (amount > 0),

    -- payment_method must be one of the known values
    CONSTRAINT chk_payments_method
        CHECK (payment_method IN ('upi', 'card', 'netbanking', 'wallet')),

    -- paid_at must be set when payment succeeded
    CONSTRAINT chk_payments_paid_at
        CHECK (payment_status <> 'success' OR paid_at IS NOT NULL)
);

COMMENT ON TABLE payments IS
    'On success: set bookings.status=confirmed, trip_seats.status=booked. '
    'On failed: release trip_seats (set status=available). Enforced at application level.';


-- ------------------------------------------------------------
-- 2.22  transactions
-- ------------------------------------------------------------
CREATE TABLE transactions (
    id               SERIAL         PRIMARY KEY,
    booking_id       INT            NOT NULL,
    operator_id      INT            NOT NULL,
    total_amount     NUMERIC(10,2)  NOT NULL,
    platform_fee     NUMERIC(10,2)  NOT NULL,
    operator_earning NUMERIC(10,2)  NOT NULL,
    created_at       TIMESTAMP      NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMP      NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_transactions_booking_id       UNIQUE (booking_id),   -- one transaction per booking
    CONSTRAINT fk_transactions_booking          FOREIGN KEY (booking_id)  REFERENCES bookings  (id),
    CONSTRAINT fk_transactions_operator         FOREIGN KEY (operator_id) REFERENCES operators (id),

    CONSTRAINT chk_transactions_total_amount_positive
        CHECK (total_amount > 0),

    CONSTRAINT chk_transactions_platform_fee_non_negative
        CHECK (platform_fee >= 0),

    CONSTRAINT chk_transactions_operator_earning_non_negative
        CHECK (operator_earning >= 0),

    -- operator_earning must equal total_amount - platform_fee
    CONSTRAINT chk_transactions_earning_matches
        CHECK (ABS(operator_earning - (total_amount - platform_fee)) < 0.01)
);

COMMENT ON TABLE transactions IS 'Created only when payments.payment_status = success. operator_earning = total_amount - platform_fee enforced at DB level.';


-- ------------------------------------------------------------
-- 2.23  bus_changes
-- ------------------------------------------------------------
CREATE TABLE bus_changes (
    id           SERIAL            PRIMARY KEY,
    trip_id      INT               NOT NULL,
    old_bus_id   INT               NOT NULL,
    new_bus_id   INT               NOT NULL,
    change_type  change_type_enum  NOT NULL,
    reason       TEXT              NOT NULL,
    changed_by   INT               NOT NULL,
    changed_at   TIMESTAMP         NOT NULL DEFAULT NOW(),
    reverted_at  TIMESTAMP,                  -- set when temporary change is reversed

    CONSTRAINT fk_bus_changes_trip       FOREIGN KEY (trip_id)    REFERENCES trips (id),
    CONSTRAINT fk_bus_changes_old_bus    FOREIGN KEY (old_bus_id)  REFERENCES buses (id),
    CONSTRAINT fk_bus_changes_new_bus    FOREIGN KEY (new_bus_id)  REFERENCES buses (id),
    CONSTRAINT fk_bus_changes_changed_by FOREIGN KEY (changed_by)  REFERENCES users (id),

    -- Old and new bus must differ
    CONSTRAINT chk_bus_changes_different_buses
        CHECK (old_bus_id <> new_bus_id),

    -- reason must not be empty string
    CONSTRAINT chk_bus_changes_reason_not_empty
        CHECK (TRIM(reason) <> ''),

    -- reverted_at must be NULL for permanent changes
    CONSTRAINT chk_bus_changes_permanent_no_revert
        CHECK (change_type <> 'permanent' OR reverted_at IS NULL),

    -- reverted_at must be after changed_at when set
    CONSTRAINT chk_bus_changes_revert_after_change
        CHECK (reverted_at IS NULL OR reverted_at > changed_at)
);

COMMENT ON TABLE bus_changes IS
    'On bus change: update trips.bus_id, regenerate trip_seats, re-map booking_seats, notify affected passengers.';


-- ------------------------------------------------------------
-- 2.24  notifications
-- ------------------------------------------------------------
CREATE TABLE notifications (
    id             SERIAL                    PRIMARY KEY,
    user_id        INT                       NOT NULL,
    title          VARCHAR(200)              NOT NULL,
    message        TEXT                      NOT NULL,
    type           VARCHAR(50)               NOT NULL,
    reference_type reference_type_enum,
    reference_id   INT,
    channel        notification_channel_enum NOT NULL DEFAULT 'in_app',
    is_read        BOOLEAN                   NOT NULL DEFAULT FALSE,
    created_at     TIMESTAMP                 NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_notifications_user FOREIGN KEY (user_id) REFERENCES users (id),

    -- reference_type and reference_id must both be set or both be NULL
    CONSTRAINT chk_notifications_reference_pair
        CHECK (
            (reference_type IS NULL AND reference_id IS NULL) OR
            (reference_type IS NOT NULL AND reference_id IS NOT NULL)
        )
);

COMMENT ON TABLE notifications IS
    'When admin blocks an operator: notify all users with confirmed bookings on that operator. '
    'reference_type=operator, reference_id=operator.id. No monetary refund — notification only.';


-- ------------------------------------------------------------
-- 2.25  audit_logs
-- ------------------------------------------------------------
CREATE TABLE audit_logs (
    id           SERIAL           PRIMARY KEY,
    actor_id     INT              NOT NULL,
    action       VARCHAR(100)     NOT NULL,
    entity_type  entity_type_enum NOT NULL,
    entity_id    INT              NOT NULL,
    old_value    JSONB,            -- state before action; NULL for CREATE actions
    new_value    JSONB,            -- state after action;  NULL for DELETE actions
    ip_address   VARCHAR(45),      -- IPv4 or IPv6; NULL for system-generated actions
    created_at   TIMESTAMP        NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_audit_logs_actor FOREIGN KEY (actor_id) REFERENCES users (id)
);

COMMENT ON TABLE audit_logs IS
    'Immutable log — rows must NEVER be updated or deleted. '
    'Covers all admin and operator actions for full traceability.';


-- ============================================================
-- 3. ADDITIONAL INDEXES  (performance-critical query paths)
-- ============================================================

-- users
CREATE INDEX idx_users_role_id        ON users (role_id);

-- operators
CREATE INDEX idx_operators_status     ON operators (approval_status);

-- operator_documents
CREATE INDEX idx_op_docs_operator_id  ON operator_documents (operator_id);

-- platform_fee_config
CREATE INDEX idx_platform_fee_active  ON platform_fee_config (is_active);

-- buses
CREATE INDEX idx_buses_operator_id    ON buses (operator_id);

-- seats
CREATE INDEX idx_seats_bus_id         ON seats (bus_id);

-- trip_schedules
CREATE INDEX idx_schedules_operator   ON trip_schedules (operator_id);
CREATE INDEX idx_schedules_active     ON trip_schedules (is_active);

-- trips  (most queried table — several composite indexes)
CREATE INDEX idx_trips_bus_departure  ON trips (bus_id, departure_time);
CREATE INDEX idx_trips_operator_id    ON trips (operator_id);
CREATE INDEX idx_trips_route_id       ON trips (route_id);
CREATE INDEX idx_trips_status         ON trips (status);
CREATE INDEX idx_trips_schedule_id    ON trips (schedule_id);

-- trip_seats  (hot path: seat availability checks)
CREATE INDEX idx_trip_seats_trip_id         ON trip_seats (trip_id);
CREATE INDEX idx_trip_seats_status          ON trip_seats (status);
CREATE INDEX idx_trip_seats_locked_until    ON trip_seats (locked_until)
    WHERE status = 'reserved';               -- partial index — only reserved seats expire

-- seat_pricing
-- (unique index already created inline as CONSTRAINT uq_seat_pricing_trip_seat)

-- bookings
CREATE INDEX idx_bookings_user_id     ON bookings (user_id);
CREATE INDEX idx_bookings_trip_id     ON bookings (trip_id);
CREATE INDEX idx_bookings_status      ON bookings (status);
CREATE INDEX idx_bookings_date        ON bookings (booking_date DESC);

-- booking_seats
CREATE INDEX idx_booking_seats_booking_id ON booking_seats (booking_id);
CREATE INDEX idx_booking_seats_seat_id    ON booking_seats (seat_id);

-- payments
CREATE INDEX idx_payments_booking_id      ON payments (booking_id);
CREATE INDEX idx_payments_status          ON payments (payment_status);

-- transactions
CREATE INDEX idx_transactions_operator    ON transactions (operator_id);
CREATE INDEX idx_transactions_created_at  ON transactions (created_at DESC);

-- bus_changes
CREATE INDEX idx_bus_changes_trip_id      ON bus_changes (trip_id);
CREATE INDEX idx_bus_changes_old_bus_id   ON bus_changes (old_bus_id);
CREATE INDEX idx_bus_changes_new_bus_id   ON bus_changes (new_bus_id);

-- notifications
CREATE INDEX idx_notifications_user_id    ON notifications (user_id);
CREATE INDEX idx_notifications_ref        ON notifications (reference_type, reference_id);
CREATE INDEX idx_notifications_unread     ON notifications (user_id, is_read)
    WHERE is_read = FALSE;                   -- partial index — only unread rows

-- audit_logs
CREATE INDEX idx_audit_logs_actor         ON audit_logs (actor_id);
CREATE INDEX idx_audit_logs_entity        ON audit_logs (entity_type, entity_id);
CREATE INDEX idx_audit_logs_action        ON audit_logs (action);
CREATE INDEX idx_audit_logs_created_at    ON audit_logs (created_at DESC);

-- operator_offices
CREATE INDEX idx_op_offices_operator_id   ON operator_offices (operator_id);


-- ============================================================
-- 4. SEED DATA — Roles  (required before any user can be created)
-- ============================================================

INSERT INTO roles (name) VALUES
    ('admin'),
    ('operator'),
    ('user')
ON CONFLICT (name) DO NOTHING;


-- ============================================================
-- 5. HELPFUL VIEWS
-- ============================================================

-- Available seats for a given trip (used by seat selection screen)
CREATE VIEW v_available_trip_seats AS
SELECT
    ts.id            AS trip_seat_id,
    ts.trip_id,
    ts.seat_id,
    ts.status,
    ts.locked_until,
    s.seat_number,
    s.row,
    s."column",
    s.deck,
    s.seat_type,
    COALESCE(sp.price, t.base_fare) AS effective_price
FROM trip_seats ts
JOIN seats       s  ON s.id  = ts.seat_id
JOIN trips       t  ON t.id  = ts.trip_id
LEFT JOIN seat_pricing sp ON sp.trip_id = ts.trip_id AND sp.seat_id = ts.seat_id
WHERE ts.status = 'available';

COMMENT ON VIEW v_available_trip_seats IS 'Shows available seats with effective price (seat-specific or base fare fallback).';


-- Operator revenue summary (for operator dashboard)
CREATE VIEW v_operator_revenue AS
SELECT
    t.operator_id,
    COUNT(DISTINCT t.booking_id)   AS total_bookings,
    SUM(t.total_amount)            AS gross_revenue,
    SUM(t.platform_fee)            AS total_platform_fees,
    SUM(t.operator_earning)        AS net_earning,
    DATE_TRUNC('month', t.created_at) AS month
FROM transactions t
GROUP BY t.operator_id, DATE_TRUNC('month', t.created_at);

COMMENT ON VIEW v_operator_revenue IS 'Monthly revenue breakdown per operator.';


-- Platform overall revenue summary (for admin dashboard)
CREATE VIEW v_platform_revenue AS
SELECT
    COUNT(DISTINCT booking_id)  AS total_bookings,
    SUM(total_amount)           AS gross_revenue,
    SUM(platform_fee)           AS platform_earnings,
    SUM(operator_earning)       AS operator_payouts,
    DATE_TRUNC('month', created_at) AS month
FROM transactions
GROUP BY DATE_TRUNC('month', created_at);

COMMENT ON VIEW v_platform_revenue IS 'Monthly platform-wide revenue summary for admin dashboard.';


COMMIT;

-- ============================================================
-- END OF SCHEMA
-- ============================================================