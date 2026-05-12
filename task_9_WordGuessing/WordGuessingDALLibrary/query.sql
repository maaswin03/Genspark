CREATE TABLE IF NOT EXISTS users(
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(50) NOT NULL,
    createdat TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS results(
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    total_attempt INT NOT NULL CHECK(total_attempt > 0 AND total_attempt < 7),
    word VARCHAR(5) NOT NULL,
    score INT NOT NULL CHECK(score >= 0 AND score <= 120),
    played_at TIMESTAMP NOT NULL,
    FOREIGN KEY (username) REFERENCES users(username)
);