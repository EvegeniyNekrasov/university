CREATE SCHEMA IF NOT EXISTS auth;

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
