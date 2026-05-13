CREATE TABLE users (
    userid SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    email TEXT NOT NULL UNIQUE,
    phonenumber VARCHAR(10) NOT NULL,
    createdat TIMESTAMP NOT NULL
);

CREATE TYPE notification_type AS ENUM (
    'EmailNotification',
    'SMSNotification'
);

CREATE TABLE notifications (
    messageid SERIAL PRIMARY KEY,
    message TEXT NOT NULL,
    sendedat TIMESTAMP NOT NULL,
    notificationtype notification_type NOT NULL,
    notificationsent BOOLEAN NOT NULL DEFAULT FALSE,
    receiverid INT NOT NULL,
    CONSTRAINT fk_receiver FOREIGN KEY (receiverid) REFERENCES users(userid) ON DELETE CASCADE
);