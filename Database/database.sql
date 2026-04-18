BEGIN;

-- Las esquemas
CREATE SCHEMA IF NOT EXISTS auth;
CREATE SCHEMA IF NOT EXISTS core;
CREATE SCHEMA IF NOT EXISTS academic;
CREATE SCHEMA IF NOT EXISTS campus;


CREATE OR REPLACE FUNCTION public.set_updated_utc()
RETURN TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_utc = now();
    RETURN NEW;
END;
$$;

CREATE TABLE IF NOT EXISTS auth.users
(
    id                  uuid primary key,
    email               text not null,
    normalized_email    text not null unique,
    password_hash       text not null,
    display_name        text null,
    is_active           boolean not null default true,
    email_confirmed     boolean not null default true,
    access_failed_count integer not null default 0,
    lockout_end_utc     timestamptz null,
    security_stamp      text not null,
    created_utc         timestamptz not null default now(),
    updated_utc         timestamptz not null default now(),
    last_login_utc      timestamptz null
);

CREATE TABLE IF NOT EXISTS auth.roles
(
    id                  uuid primary key,
    name                text not null unique
);

CREATE TABLE IF NOT EXISTS auth.user_roles
(
    user_id             uuid not null references auth.users(id) on delete cascade,
    role_id             uuid not null references auth.roles(id) on delete cascade,
    primary key (user_id, role_id)
);


CREATE INDEX IF NOT EXISTS ix_auth_users_is_active
    ON auth.users(is_active);

CREATE INDEX IF NOT EXISTS ix_auth_users_is_active
    ON auth.users(is_active);

CREATE INDEX IF NOT EXISTS ix_auth_users_is_active
    ON auth.users(is_active);

INSERT INTO auth.roles (id, name)
VALUES
    ('00000000-0000-0000-0000-000000000001', 'Admin'),
    ('00000000-0000-0000-0000-000000000002', 'Student'),
    ('00000000-0000-0000-0000-000000000003', 'Professor')
ON CONFLICT (name) DO NOTHING;


CREATE TABLE IF NOT EXISTS core.persons
(
    id              uuid PRIMARY KEY,
    user_id         uuid NOT NULL UNIQUE REFERENCES auth.users(id),
    first_name      text NOT NULL,
    last_name       text NOT NULL,
    document_number text NULL,
    birth_date      date NULL,
    phone           text NULL,
    address         text NULL,
    created_utc     timestamptz NOT NULL DEFAULT now(),
    updated_utc     timestamptz NOT NULL DEFAULT now()
);


CREATE INDEX IF NOT EXISTS ix_core_persons_last_name
    ON core.persons(last_name);

CREATE INDEX IF NOT EXISTS ix_core_persons_document_number
    ON core.persons(document_number)
    WHERE document_number IS NOT NULL;


CREATE TABLE IF NOT EXISTS academic.students
(
    id              uuid PRIMARY KEY,
    person_id       uuid NOT NULL UNIQUE REFERENCES core.persons(id) ON DELETE CASCADE,
    student_code    text NOT NULL UNIQUE,
    enrollment_date date NOT NULL,
    status          text NOT NULL,
    created_utc     timestamptz NOT NULL DEFAULT now(),
    updated_utc     timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_students_status
        CHECK (status IN ('Active', 'Inactive', 'Suspended', 'Graduated'))
);

CREATE INDEX IF NOT EXISTS ix_academic_students_status
    ON academic.students(status);

CREATE TABLE IF NOT EXISTS academic.professors
(
    id              uuid PRIMARY KEY,
    person_id       uuid NOT NULL UNIQUE REFERENCES core.persons(id) ON DELETE CASCADE,
    employee_code   text NOT NULL UNIQUE,
    specialization  text NULL,
    status          text NOT NULL,
    created_utc     timestamptz NOT NULL DEFAULT now(),
    updated_utc     timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_professors_status
        CHECK (status IN ('Active', 'Inactive', 'OnLeave'))
);

CREATE INDEX IF NOT EXISTS ix_academic_professors_status
    ON academic.professors(status);

CREATE TABLE IF NOT EXISTS academic.academic_years
(
    id          uuid PRIMARY KEY,
    name        text NOT NULL UNIQUE,
    start_date  date NOT NULL,
    end_date    date NOT NULL,
    is_active   boolean NOT NULL DEFAULT false,
    created_utc timestamptz NOT NULL DEFAULT now(),
    updated_utc timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_years_date_range
        CHECK (start_date < end_date)
);

-- SOLO UN CURSO ACTIVADO A LA VEZ
CREATE UNIQUE INDEX IF NOT EXISTS ux_academic_years_single_active
    ON academic.academic_years (is_active)
    WHERE is_active = true;


CREATE TABLE IF NOT EXISTS academic.terms
(
    id               uuid PRIMARY KEY,
    academic_year_id uuid NOT NULL REFERENCES academic.academic_years(id),
    name             text NOT NULL,
    sort_order       smallint NOT NULL,
    start_date       date NOT NULL,
    end_date         date NOT NULL,
    created_utc      timestamptz NOT NULL DEFAULT now(),
    updated_utc      timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_terms_date_range
        CHECK (start_date < end_date),
    CONSTRAINT uq_academic_terms_name_per_year
        UNIQUE (academic_year_id, name),
    CONSTRAINT uq_academic_terms_order_per_year
        UNIQUE (academic_year_id, sort_order),
    CONSTRAINT uq_academic_terms_id_year
        UNIQUE (id, academic_year_id)
);

CREATE INDEX IF NOT EXISTS ix_academic_terms_academic_year_id
    ON academic.terms(academic_year_id);


CREATE TABLE IF NOT EXISTS academic.courses
(
    id          uuid PRIMARY KEY,
    code        text NOT NULL UNIQUE,
    name        text NOT NULL,
    credits     numeric(4,2) NOT NULL,
    description text NULL,
    is_active   boolean NOT NULL DEFAULT true,
    created_utc timestamptz NOT NULL DEFAULT now(),
    updated_utc timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_courses_credits_positive
        CHECK (credits > 0)
);

CREATE INDEX IF NOT EXISTS ix_academic_courses_name
    ON academic.courses(name);

CREATE INDEX IF NOT EXISTS ix_academic_courses_is_active
    ON academic.courses(is_active);


CREATE TABLE IF NOT EXISTS academic.course_offerings
(
    id               uuid PRIMARY KEY,
    course_id        uuid NOT NULL REFERENCES academic.courses(id),
    academic_year_id uuid NOT NULL REFERENCES academic.academic_years(id),
    term_id          uuid NOT NULL,
    max_seats        integer NOT NULL,
    status           text NOT NULL,
    created_utc      timestamptz NOT NULL DEFAULT now(),
    updated_utc      timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_course_offerings_max_seats_positive
        CHECK (max_seats > 0),
    CONSTRAINT ck_academic_course_offerings_status
        CHECK (status IN ('Draft', 'Open', 'Closed', 'Cancelled')),
    CONSTRAINT uq_academic_course_offerings_course_year_term
        UNIQUE (course_id, academic_year_id, term_id),
    CONSTRAINT fk_academic_course_offerings_term_year
        FOREIGN KEY (term_id, academic_year_id)
        REFERENCES academic.terms(id, academic_year_id)
);

CREATE INDEX IF NOT EXISTS ix_academic_course_offerings_academic_year_id
    ON academic.course_offerings(academic_year_id);

CREATE INDEX IF NOT EXISTS ix_academic_course_offerings_term_id
    ON academic.course_offerings(term_id);

CREATE INDEX IF NOT EXISTS ix_academic_course_offerings_status
    ON academic.course_offerings(status);

CREATE TABLE IF NOT EXISTS academic.sections
(
    id                uuid PRIMARY KEY,
    course_offering_id uuid NOT NULL REFERENCES academic.course_offerings(id),
    code              text NOT NULL,
    modality          text NOT NULL,
    capacity          integer NOT NULL,
    status            text NOT NULL,
    schedule_notes    text NULL,
    created_utc       timestamptz NOT NULL DEFAULT now(),
    updated_utc       timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_sections_capacity_positive
        CHECK (capacity > 0),
    CONSTRAINT ck_academic_sections_modality
        CHECK (modality IN ('OnSite', 'Online', 'Hybrid')),
    CONSTRAINT ck_academic_sections_status
        CHECK (status IN ('Draft', 'Open', 'Closed', 'Cancelled')),
    CONSTRAINT uq_academic_sections_offering_code
        UNIQUE (course_offering_id, code)
);

CREATE INDEX IF NOT EXISTS ix_academic_sections_course_offering_id
    ON academic.sections(course_offering_id);

CREATE INDEX IF NOT EXISTS ix_academic_sections_status
    ON academic.sections(status);

CREATE TABLE IF NOT EXISTS academic.teaching_assignments
(
    id            uuid PRIMARY KEY,
    professor_id  uuid NOT NULL REFERENCES academic.professors(id),
    section_id    uuid NOT NULL REFERENCES academic.sections(id),
    assigned_utc  timestamptz NOT NULL DEFAULT now(),
    created_utc   timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_academic_teaching_assignments_professor_section
        UNIQUE (professor_id, section_id)
);

CREATE INDEX IF NOT EXISTS ix_academic_teaching_assignments_section_id
    ON academic.teaching_assignments(section_id);

CREATE TABLE IF NOT EXISTS academic.enrollments
(
    id               uuid PRIMARY KEY,
    student_id       uuid NOT NULL REFERENCES academic.students(id),
    academic_year_id uuid NOT NULL REFERENCES academic.academic_years(id),
    enrollment_utc   timestamptz NOT NULL DEFAULT now(),
    status           text NOT NULL,
    created_utc      timestamptz NOT NULL DEFAULT now(),
    updated_utc      timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_enrollments_status
        CHECK (status IN ('Draft', 'Confirmed', 'Cancelled')),
    CONSTRAINT uq_academic_enrollments_student_year
        UNIQUE (student_id, academic_year_id)
);

CREATE INDEX IF NOT EXISTS ix_academic_enrollments_academic_year_id
    ON academic.enrollments(academic_year_id);

CREATE INDEX IF NOT EXISTS ix_academic_enrollments_status
    ON academic.enrollments(status);

CREATE TABLE IF NOT EXISTS academic.enrollment_items
(
    id             uuid PRIMARY KEY,
    enrollment_id  uuid NOT NULL REFERENCES academic.enrollments(id) ON DELETE CASCADE,
    section_id     uuid NOT NULL REFERENCES academic.sections(id),
    status         text NOT NULL,
    registered_utc timestamptz NOT NULL DEFAULT now(),
    created_utc    timestamptz NOT NULL DEFAULT now(),
    updated_utc    timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_enrollment_items_status
        CHECK (status IN ('Active', 'Dropped', 'Cancelled')),
    CONSTRAINT uq_academic_enrollment_items_enrollment_section
        UNIQUE (enrollment_id, section_id)
);

CREATE INDEX IF NOT EXISTS ix_academic_enrollment_items_section_id
    ON academic.enrollment_items(section_id);

CREATE INDEX IF NOT EXISTS ix_academic_enrollment_items_status
    ON academic.enrollment_items(status);

CREATE TABLE IF NOT EXISTS campus.buildings
(
    id          uuid PRIMARY KEY,
    code        text NOT NULL UNIQUE,
    name        text NOT NULL UNIQUE,
    created_utc timestamptz NOT NULL DEFAULT now(),
    updated_utc timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS campus.rooms
(
    id          uuid PRIMARY KEY,
    building_id uuid NOT NULL REFERENCES campus.buildings(id),
    name        text NOT NULL,
    capacity    integer NOT NULL,
    created_utc timestamptz NOT NULL DEFAULT now(),
    updated_utc timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_campus_rooms_capacity_positive
        CHECK (capacity > 0),
    CONSTRAINT uq_campus_rooms_building_name
        UNIQUE (building_id, name)
);

CREATE INDEX IF NOT EXISTS ix_campus_rooms_building_id
    ON campus.rooms(building_id);

CREATE TABLE IF NOT EXISTS academic.schedule_slots
(
    id          uuid PRIMARY KEY,
    section_id  uuid NOT NULL REFERENCES academic.sections(id) ON DELETE CASCADE,
    room_id     uuid NOT NULL REFERENCES campus.rooms(id),
    day_of_week smallint NOT NULL,
    start_time  time NOT NULL,
    end_time    time NOT NULL,
    created_utc timestamptz NOT NULL DEFAULT now(),
    updated_utc timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_schedule_slots_day_of_week
        CHECK (day_of_week BETWEEN 1 AND 7),
    CONSTRAINT ck_academic_schedule_slots_time_range
        CHECK (start_time < end_time),
    CONSTRAINT uq_academic_schedule_slots_exact_duplicate
        UNIQUE (section_id, room_id, day_of_week, start_time, end_time)
);

CREATE INDEX IF NOT EXISTS ix_academic_schedule_slots_section_id
    ON academic.schedule_slots(section_id);

CREATE INDEX IF NOT EXISTS ix_academic_schedule_slots_room_id
    ON academic.schedule_slots(room_id);

CREATE INDEX IF NOT EXISTS ix_academic_schedule_slots_day_start_end
    ON academic.schedule_slots(day_of_week, start_time, end_time);


CREATE TABLE IF NOT EXISTS academic.assessments
(
    id           uuid PRIMARY KEY,
    section_id    uuid NOT NULL REFERENCES academic.sections(id) ON DELETE CASCADE,
    title        text NOT NULL,
    description  text NULL,
    weight       numeric(5,2) NOT NULL,
    max_score    numeric(5,2) NOT NULL,
    due_utc      timestamptz NULL,
    is_published boolean NOT NULL DEFAULT false,
    created_utc  timestamptz NOT NULL DEFAULT now(),
    updated_utc  timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_assessments_weight
        CHECK (weight > 0 AND weight <= 100),
    CONSTRAINT ck_academic_assessments_max_score
        CHECK (max_score > 0)
);

CREATE INDEX IF NOT EXISTS ix_academic_assessments_section_id
    ON academic.assessments(section_id);

CREATE INDEX IF NOT EXISTS ix_academic_assessments_is_published
    ON academic.assessments(is_published);

CREATE INDEX IF NOT EXISTS ix_academic_assessments_due_utc
    ON academic.assessments(due_utc);

CREATE TABLE IF NOT EXISTS academic.grades
(
    id            uuid PRIMARY KEY,
    assessment_id uuid NOT NULL REFERENCES academic.assessments(id) ON DELETE CASCADE,
    student_id    uuid NOT NULL REFERENCES academic.students(id),
    score         numeric(5,2) NOT NULL,
    feedback      text NULL,
    graded_utc    timestamptz NOT NULL DEFAULT now(),
    created_utc   timestamptz NOT NULL DEFAULT now(),
    updated_utc   timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_academic_grades_score_non_negative
        CHECK (score >= 0),
    CONSTRAINT uq_academic_grades_assessment_student
        UNIQUE (assessment_id, student_id)
);

CREATE INDEX IF NOT EXISTS ix_academic_grades_student_id
    ON academic.grades(student_id);

DROP TRIGGER IF EXISTS trg_auth_users_set_updated_utc ON auth.users;
CREATE TRIGGER trg_auth_users_set_updated_utc
BEFORE UPDATE ON auth.users
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_core_persons_set_updated_utc ON core.persons;
CREATE TRIGGER trg_core_persons_set_updated_utc
BEFORE UPDATE ON core.persons
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_students_set_updated_utc ON academic.students;
CREATE TRIGGER trg_academic_students_set_updated_utc
BEFORE UPDATE ON academic.students
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_professors_set_updated_utc ON academic.professors;
CREATE TRIGGER trg_academic_professors_set_updated_utc
BEFORE UPDATE ON academic.professors
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_academic_years_set_updated_utc ON academic.academic_years;
CREATE TRIGGER trg_academic_academic_years_set_updated_utc
BEFORE UPDATE ON academic.academic_years
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_terms_set_updated_utc ON academic.terms;
CREATE TRIGGER trg_academic_terms_set_updated_utc
BEFORE UPDATE ON academic.terms
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_courses_set_updated_utc ON academic.courses;
CREATE TRIGGER trg_academic_courses_set_updated_utc
BEFORE UPDATE ON academic.courses
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_course_offerings_set_updated_utc ON academic.course_offerings;
CREATE TRIGGER trg_academic_course_offerings_set_updated_utc
BEFORE UPDATE ON academic.course_offerings
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_sections_set_updated_utc ON academic.sections;
CREATE TRIGGER trg_academic_sections_set_updated_utc
BEFORE UPDATE ON academic.sections
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_enrollments_set_updated_utc ON academic.enrollments;
CREATE TRIGGER trg_academic_enrollments_set_updated_utc
BEFORE UPDATE ON academic.enrollments
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_enrollment_items_set_updated_utc ON academic.enrollment_items;
CREATE TRIGGER trg_academic_enrollment_items_set_updated_utc
BEFORE UPDATE ON academic.enrollment_items
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_campus_buildings_set_updated_utc ON campus.buildings;
CREATE TRIGGER trg_campus_buildings_set_updated_utc
BEFORE UPDATE ON campus.buildings
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_campus_rooms_set_updated_utc ON campus.rooms;
CREATE TRIGGER trg_campus_rooms_set_updated_utc
BEFORE UPDATE ON campus.rooms
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_schedule_slots_set_updated_utc ON academic.schedule_slots;
CREATE TRIGGER trg_academic_schedule_slots_set_updated_utc
BEFORE UPDATE ON academic.schedule_slots
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_assessments_set_updated_utc ON academic.assessments;
CREATE TRIGGER trg_academic_assessments_set_updated_utc
BEFORE UPDATE ON academic.assessments
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

DROP TRIGGER IF EXISTS trg_academic_grades_set_updated_utc ON academic.grades;
CREATE TRIGGER trg_academic_grades_set_updated_utc
BEFORE UPDATE ON academic.grades
FOR EACH ROW
EXECUTE FUNCTION public.set_updated_utc();

COMMIT;


-- SOME INITIAL INSERTS
insert into auth.roles (id, name)
values
    (gen_random_uuid(), 'Admin'),
    (gen_random_uuid(), 'User')
on conflict (name) do nothing;

insert into auth.user_roles (user_id, role_id)
select u.id, r.id
from auth.users u
cross join auth.roles r
where u.normalized_email = 'ADMIN@LOCAL.TEST'
  and r.name = 'Admin'
on conflict do nothing;

insert into auth.user_roles (user_id, role_id)
select u.id, r.id
from auth.users u
cross join auth.roles r
where u.normalized_email = 'USER@LOCAL.TEST'
  and r.name = 'User'
on conflict do nothing;
