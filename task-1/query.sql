CREATE TABLE IF NOT EXISTS store_details(
    id SERIAL CONSTRAINT pk_store_detail_id PRIMARY KEY,
    store_name VARCHAR(50) NOT NULL,
    store_address TEXT NOT NULL,
    store_phone_number CHAR(10) NOT NULL UNIQUE CHECK(store_phone_number ~ '^[0-9]{10}$')
);

CREATE TABLE IF NOT EXISTS movie_format(
    id SERIAL CONSTRAINT pk_movie_format_id PRIMARY KEY,
    format VARCHAR(10) NOT NULL UNIQUE CHECK(format IN ('VHS' , 'VCD' , 'DVD'))
);

CREATE TABLE IF NOT EXISTS movie_genre(
    id SERIAL CONSTRAINT pk_movie_genre_id PRIMARY KEY,
    category VARCHAR(25) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS movie_details(
    id SERIAL CONSTRAINT pk_movie_details_id PRIMARY KEY,
    title VARCHAR(100) NOT NULL UNIQUE,
    description TEXT NOT NULL,
    director_name VARCHAR(50) NOT NULL,
    lead_actor VARCHAR(50) NOT NULL,
    released_date DATE NOT NULL,
    ratings DECIMAL(2,1) NOT NULL CHECK(ratings BETWEEN 0 AND 5)
);

CREATE TABLE IF NOT EXISTS movie(
    id SERIAL CONSTRAINT pk_movie_id  PRIMARY KEY,
    movie_genre_id INT NOT NULL,
    movie_format_id INT NOT NULL,
    movie_details_id INT NOT NULL,
    CONSTRAINT fk_movie_genre_id FOREIGN KEY (movie_genre_id) REFERENCES movie_genre(id) ON DELETE RESTRICT,
    CONSTRAINT fk_movie_format_id FOREIGN KEY (movie_format_id) REFERENCES movie_format(id) ON DELETE RESTRICT,
    CONSTRAINT fk_movie_details_id FOREIGN KEY (movie_details_id) REFERENCES movie_details(id) ON DELETE CASCADE,
    UNIQUE(movie_details_id,movie_format_id);
);

CREATE TABLE IF NOT EXISTS membership_details(
    id SERIAL CONSTRAINT pk_membership_details_id PRIMARY KEY,
    membership VARCHAR(50) NOT NULL UNIQUE CHECK(membership IN ('GOLDEN','BRONZE','NO_MEMBERSHIP'))
);

CREATE TABLE IF NOT EXISTS users(
    id SERIAL CONSTRAINT pk_user_id PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    phone_number CHAR(10) NOT NULL UNIQUE CHECK(phone_number ~ '^[0-9]{10}$'),
    membership_id INT NOT NULL,
    favourite_genre_id INT NULL,
    CONSTRAINT fk_users_movie_genre_id FOREIGN KEY (favourite_genre_id) REFERENCES movie_genre(id),
    CONSTRAINT fk_users_membership_details_id FOREIGN KEY (membership_id) REFERENCES membership_details(id)
);

CREATE TABLE IF NOT EXISTS dependents(
    id SERIAL CONSTRAINT pk_dependents_id PRIMARY KEY,
    dependent_name VARCHAR(50) NOT NULL,
    relationship VARCHAR(50) NOT NULL,
    user_id INT NOT NULL,
    CONSTRAINT fk_dependents_user_id FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS rented_movies(
    id SERIAL CONSTRAINT pk_rented_movies_id PRIMARY KEY,
    user_id INT,
    dependent_id INT,
    movie_id INT NOT NULL,
    rented_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    due_date TIMESTAMP NOT NULL,
    returned_at TIMESTAMP,
    CONSTRAINT fk_rented_user_id FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT fk_rented_dependent_id FOREIGN KEY (dependent_id) REFERENCES dependents(id),
    CONSTRAINT fk_rented_movie_id FOREIGN KEY (movie_id) REFERENCES movie(id),

    CHECK((user_id IS NOT NULL AND dependent_id IS NULL) OR (user_id IS NULL AND dependent_id IS NOT NULL)),
    CHECK (due_date > rented_at),
    CHECK (returned_at IS NULL OR returned_at >= rented_at)
);