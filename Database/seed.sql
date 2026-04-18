BEGIN;

INSERT INTO auth.roles (id, name)
VALUES
    ('00000000-0000-0000-0000-000000000001', 'Admin'),
    ('00000000-0000-0000-0000-000000000002', 'Student'),
    ('00000000-0000-0000-0000-000000000003', 'Professor')
ON CONFLICT (name) DO NOTHING;

INSERT INTO core.persons
(
    id,
    user_id,
    first_name,
    last_name,
    document_number,
    birth_date,
    phone,
    address,
    created_utc,
    updated_utc
)
VALUES
    (
        '20000000-0000-0000-0000-000000000001',
        '10000000-0000-0000-0000-000000000001',
        'System',
        'Admin',
        'ADM-0001',
        '1985-01-10',
        '+34 600 000 001',
        'Campus Central',
        now(),
        now()
    ),
    (
        '20000000-0000-0000-0000-000000000101',
        '10000000-0000-0000-0000-000000000101',
        'Laura',
        'Gómez',
        'PROF-0001',
        '1980-05-14',
        '+34 600 000 101',
        'Madrid',
        now(),
        now()
    ),
    (
        '20000000-0000-0000-0000-000000000102',
        '10000000-0000-0000-0000-000000000102',
        'Sergio',
        'Ruiz',
        'PROF-0002',
        '1978-09-03',
        '+34 600 000 102',
        'Madrid',
        now(),
        now()
    ),
    (
        '20000000-0000-0000-0000-000000000201',
        '10000000-0000-0000-0000-000000000201',
        'Ana',
        'Martín',
        'STU-0001',
        '2004-03-21',
        '+34 600 000 201',
        'Alcalá de Henares',
        now(),
        now()
    ),
    (
        '20000000-0000-0000-0000-000000000202',
        '10000000-0000-0000-0000-000000000202',
        'Pablo',
        'Sánchez',
        'STU-0002',
        '2003-11-07',
        '+34 600 000 202',
        'Madrid',
        now(),
        now()
    ),
    (
        '20000000-0000-0000-0000-000000000203',
        '10000000-0000-0000-0000-000000000203',
        'Lucía',
        'Fernández',
        'STU-0003',
        '2004-06-01',
        '+34 600 000 203',
        'Torrejón de Ardoz',
        now(),
        now()
    )
ON CONFLICT (user_id) DO NOTHING;


INSERT INTO academic.students
(
    id,
    person_id,
    student_code,
    enrollment_date,
    status,
    created_utc,
    updated_utc
)
VALUES
    (
        '30000000-0000-0000-0000-000000000201',
        '20000000-0000-0000-0000-000000000201',
        'ST2026001',
        '2026-09-01',
        'Active',
        now(),
        now()
    ),
    (
        '30000000-0000-0000-0000-000000000202',
        '20000000-0000-0000-0000-000000000202',
        'ST2026002',
        '2026-09-01',
        'Active',
        now(),
        now()
    ),
    (
        '30000000-0000-0000-0000-000000000203',
        '20000000-0000-0000-0000-000000000203',
        'ST2026003',
        '2026-09-01',
        'Active',
        now(),
        now()
    )
ON CONFLICT (person_id) DO NOTHING;


INSERT INTO academic.professors
(
    id,
    person_id,
    employee_code,
    specialization,
    status,
    created_utc,
    updated_utc
)
VALUES
    (
        '40000000-0000-0000-0000-000000000101',
        '20000000-0000-0000-0000-000000000101',
        'PR2026001',
        'Bases de Datos',
        'Active',
        now(),
        now()
    ),
    (
        '40000000-0000-0000-0000-000000000102',
        '20000000-0000-0000-0000-000000000102',
        'PR2026002',
        'Desarrollo Web',
        'Active',
        now(),
        now()
    )
ON CONFLICT (person_id) DO NOTHING;


INSERT INTO academic.academic_years
(
    id,
    name,
    start_date,
    end_date,
    is_active,
    created_utc,
    updated_utc
)
VALUES
    (
        '50000000-0000-0000-0000-000000000001',
        '2026/2027',
        '2026-09-01',
        '2027-06-30',
        true,
        now(),
        now()
    )
ON CONFLICT (name) DO NOTHING;


INSERT INTO academic.terms
(
    id,
    academic_year_id,
    name,
    sort_order,
    start_date,
    end_date,
    created_utc,
    updated_utc
)
VALUES
    (
        '51000000-0000-0000-0000-000000000001',
        '50000000-0000-0000-0000-000000000001',
        'Primer semestre',
        1,
        '2026-09-01',
        '2027-01-31',
        now(),
        now()
    ),
    (
        '51000000-0000-0000-0000-000000000002',
        '50000000-0000-0000-0000-000000000001',
        'Segundo semestre',
        2,
        '2027-02-01',
        '2027-06-30',
        now(),
        now()
    )
ON CONFLICT (academic_year_id, name) DO NOTHING;


INSERT INTO academic.courses
(
    id,
    code,
    name,
    credits,
    description,
    is_active,
    created_utc,
    updated_utc
)
VALUES
    (
        '60000000-0000-0000-0000-000000000001',
        'DB101',
        'Bases de Datos',
        6.00,
        'Modelado relacional, SQL y fundamentos de persistencia.',
        true,
        now(),
        now()
    ),
    (
        '60000000-0000-0000-0000-000000000002',
        'PRG201',
        'Programación Web',
        6.00,
        'Desarrollo de aplicaciones web modernas.',
        true,
        now(),
        now()
    ),
    (
        '60000000-0000-0000-0000-000000000003',
        'BLZ301',
        'Interfaces con Blazor',
        4.50,
        'Construcción de frontends con Blazor.',
        true,
        now(),
        now()
    )
ON CONFLICT (code) DO NOTHING;


INSERT INTO academic.course_offerings
(
    id,
    course_id,
    academic_year_id,
    term_id,
    max_seats,
    status,
    created_utc,
    updated_utc
)
VALUES
    (
        '70000000-0000-0000-0000-000000000001',
        '60000000-0000-0000-0000-000000000001',
        '50000000-0000-0000-0000-000000000001',
        '51000000-0000-0000-0000-000000000001',
        80,
        'Open',
        now(),
        now()
    ),
    (
        '70000000-0000-0000-0000-000000000002',
        '60000000-0000-0000-0000-000000000002',
        '50000000-0000-0000-0000-000000000001',
        '51000000-0000-0000-0000-000000000001',
        60,
        'Open',
        now(),
        now()
    ),
    (
        '70000000-0000-0000-0000-000000000003',
        '60000000-0000-0000-0000-000000000003',
        '50000000-0000-0000-0000-000000000001',
        '51000000-0000-0000-0000-000000000002',
        50,
        'Draft',
        now(),
        now()
    )
ON CONFLICT (course_id, academic_year_id, term_id) DO NOTHING;


INSERT INTO academic.sections
(
    id,
    course_offering_id,
    code,
    modality,
    capacity,
    status,
    schedule_notes,
    created_utc,
    updated_utc
)
VALUES
    (
        '71000000-0000-0000-0000-000000000001',
        '70000000-0000-0000-0000-000000000001',
        'A',
        'OnSite',
        40,
        'Open',
        'Grupo de mañana',
        now(),
        now()
    ),
    (
        '71000000-0000-0000-0000-000000000002',
        '70000000-0000-0000-0000-000000000001',
        'B',
        'OnSite',
        40,
        'Open',
        'Grupo de tarde',
        now(),
        now()
    ),
    (
        '71000000-0000-0000-0000-000000000003',
        '70000000-0000-0000-0000-000000000002',
        'A',
        'Hybrid',
        30,
        'Open',
        'Sesiones híbridas',
        now(),
        now()
    ),
    (
        '71000000-0000-0000-0000-000000000004',
        '70000000-0000-0000-0000-000000000003',
        'A',
        'OnSite',
        25,
        'Draft',
        'Pendiente de publicar',
        now(),
        now()
    )
ON CONFLICT (course_offering_id, code) DO NOTHING;


INSERT INTO academic.teaching_assignments
(
    id,
    professor_id,
    section_id,
    assigned_utc,
    created_utc
)
VALUES
    (
        '72000000-0000-0000-0000-000000000001',
        '40000000-0000-0000-0000-000000000101',
        '71000000-0000-0000-0000-000000000001',
        now(),
        now()
    ),
    (
        '72000000-0000-0000-0000-000000000002',
        '40000000-0000-0000-0000-000000000101',
        '71000000-0000-0000-0000-000000000002',
        now(),
        now()
    ),
    (
        '72000000-0000-0000-0000-000000000003',
        '40000000-0000-0000-0000-000000000102',
        '71000000-0000-0000-0000-000000000003',
        now(),
        now()
    ),
    (
        '72000000-0000-0000-0000-000000000004',
        '40000000-0000-0000-0000-000000000102',
        '71000000-0000-0000-0000-000000000004',
        now(),
        now()
    )
ON CONFLICT (professor_id, section_id) DO NOTHING;


INSERT INTO campus.buildings
(
    id,
    code,
    name,
    created_utc,
    updated_utc
)
VALUES
    (
        '80000000-0000-0000-0000-000000000001',
        'MAIN',
        'Edificio Principal',
        now(),
        now()
    ),
    (
        '80000000-0000-0000-0000-000000000002',
        'TECH',
        'Edificio Tecnológico',
        now(),
        now()
    )
ON CONFLICT (code) DO NOTHING;


INSERT INTO campus.rooms
(
    id,
    building_id,
    name,
    capacity,
    created_utc,
    updated_utc
)
VALUES
    (
        '81000000-0000-0000-0000-000000000001',
        '80000000-0000-0000-0000-000000000001',
        'Aula 101',
        50,
        now(),
        now()
    ),
    (
        '81000000-0000-0000-0000-000000000002',
        '80000000-0000-0000-0000-000000000001',
        'Aula 102',
        50,
        now(),
        now()
    ),
    (
        '81000000-0000-0000-0000-000000000003',
        '80000000-0000-0000-0000-000000000002',
        'Lab 201',
        30,
        now(),
        now()
    )
ON CONFLICT (building_id, name) DO NOTHING;


INSERT INTO academic.schedule_slots
(
    id,
    section_id,
    room_id,
    day_of_week,
    start_time,
    end_time,
    created_utc,
    updated_utc
)
VALUES
    (
        '82000000-0000-0000-0000-000000000001',
        '71000000-0000-0000-0000-000000000001',
        '81000000-0000-0000-0000-000000000001',
        1,
        '09:00',
        '11:00',
        now(),
        now()
    ),
    (
        '82000000-0000-0000-0000-000000000002',
        '71000000-0000-0000-0000-000000000001',
        '81000000-0000-0000-0000-000000000001',
        3,
        '09:00',
        '11:00',
        now(),
        now()
    ),
    (
        '82000000-0000-0000-0000-000000000003',
        '71000000-0000-0000-0000-000000000002',
        '81000000-0000-0000-0000-000000000002',
        2,
        '16:00',
        '18:00',
        now(),
        now()
    ),
    (
        '82000000-0000-0000-0000-000000000004',
        '71000000-0000-0000-0000-000000000002',
        '81000000-0000-0000-0000-000000000002',
        4,
        '16:00',
        '18:00',
        now(),
        now()
    ),
    (
        '82000000-0000-0000-0000-000000000005',
        '71000000-0000-0000-0000-000000000003',
        '81000000-0000-0000-0000-000000000003',
        1,
        '12:00',
        '14:00',
        now(),
        now()
    ),
    (
        '82000000-0000-0000-0000-000000000006',
        '71000000-0000-0000-0000-000000000003',
        '81000000-0000-0000-0000-000000000003',
        3,
        '12:00',
        '14:00',
        now(),
        now()
    )
ON CONFLICT (section_id, room_id, day_of_week, start_time, end_time) DO NOTHING;


INSERT INTO academic.enrollments
(
    id,
    student_id,
    academic_year_id,
    enrollment_utc,
    status,
    created_utc,
    updated_utc
)
VALUES
    (
        '90000000-0000-0000-0000-000000000201',
        '30000000-0000-0000-0000-000000000201',
        '50000000-0000-0000-0000-000000000001',
        now(),
        'Confirmed',
        now(),
        now()
    ),
    (
        '90000000-0000-0000-0000-000000000202',
        '30000000-0000-0000-0000-000000000202',
        '50000000-0000-0000-0000-000000000001',
        now(),
        'Confirmed',
        now(),
        now()
    ),
    (
        '90000000-0000-0000-0000-000000000203',
        '30000000-0000-0000-0000-000000000203',
        '50000000-0000-0000-0000-000000000001',
        now(),
        'Confirmed',
        now(),
        now()
    )
ON CONFLICT (student_id, academic_year_id) DO NOTHING;


INSERT INTO academic.enrollment_items
(
    id,
    enrollment_id,
    section_id,
    status,
    registered_utc,
    created_utc,
    updated_utc
)
VALUES
    (
        '91000000-0000-0000-0000-000000000001',
        '90000000-0000-0000-0000-000000000201',
        '71000000-0000-0000-0000-000000000001',
        'Active',
        now(),
        now(),
        now()
    ),
    (
        '91000000-0000-0000-0000-000000000002',
        '90000000-0000-0000-0000-000000000201',
        '71000000-0000-0000-0000-000000000003',
        'Active',
        now(),
        now(),
        now()
    ),
    (
        '91000000-0000-0000-0000-000000000003',
        '90000000-0000-0000-0000-000000000202',
        '71000000-0000-0000-0000-000000000001',
        'Active',
        now(),
        now(),
        now()
    ),
    (
        '91000000-0000-0000-0000-000000000004',
        '90000000-0000-0000-0000-000000000202',
        '71000000-0000-0000-0000-000000000003',
        'Active',
        now(),
        now(),
        now()
    ),
    (
        '91000000-0000-0000-0000-000000000005',
        '90000000-0000-0000-0000-000000000203',
        '71000000-0000-0000-0000-000000000002',
        'Active',
        now(),
        now(),
        now()
    )
ON CONFLICT (enrollment_id, section_id) DO NOTHING;


INSERT INTO academic.assessments
(
    id,
    section_id,
    title,
    description,
    weight,
    max_score,
    due_utc,
    is_published,
    created_utc,
    updated_utc
)
VALUES
    (
        '92000000-0000-0000-0000-000000000001',
        '71000000-0000-0000-0000-000000000001',
        'Parcial 1',
        'Primer examen parcial de Bases de Datos.',
        30.00,
        10.00,
        '2026-10-20 10:00:00+00',
        true,
        now(),
        now()
    ),
    (
        '92000000-0000-0000-0000-000000000002',
        '71000000-0000-0000-0000-000000000001',
        'Práctica SQL',
        'Entrega práctica sobre consultas SQL.',
        20.00,
        10.00,
        '2026-11-05 23:59:00+00',
        true,
        now(),
        now()
    ),
    (
        '92000000-0000-0000-0000-000000000003',
        '71000000-0000-0000-0000-000000000003',
        'Proyecto MVC',
        'Proyecto de Programación Web.',
        40.00,
        10.00,
        '2026-12-10 23:59:00+00',
        false,
        now(),
        now()
    )
ON CONFLICT DO NOTHING;


INSERT INTO academic.grades
(
    id,
    assessment_id,
    student_id,
    score,
    feedback,
    graded_utc,
    created_utc,
    updated_utc
)
VALUES
    (
        '93000000-0000-0000-0000-000000000001',
        '92000000-0000-0000-0000-000000000001',
        '30000000-0000-0000-0000-000000000201',
        8.50,
        'Buen dominio del modelo relacional.',
        now(),
        now(),
        now()
    ),
    (
        '93000000-0000-0000-0000-000000000002',
        '92000000-0000-0000-0000-000000000001',
        '30000000-0000-0000-0000-000000000202',
        7.25,
        'Buen examen, revisar normalización.',
        now(),
        now(),
        now()
    ),
    (
        '93000000-0000-0000-0000-000000000003',
        '92000000-0000-0000-0000-000000000002',
        '30000000-0000-0000-0000-000000000201',
        9.00,
        'Consultas SQL correctas y bien estructuradas.',
        now(),
        now(),
        now()
    ),
    (
        '93000000-0000-0000-0000-000000000004',
        '92000000-0000-0000-0000-000000000002',
        '30000000-0000-0000-0000-000000000202',
        8.00,
        'Buen trabajo general.',
        now(),
        now(),
        now()
    )
ON CONFLICT (assessment_id, student_id) DO NOTHING;

COMMIT;