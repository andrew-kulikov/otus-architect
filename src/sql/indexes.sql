alter table User drop index idx_username;
alter table UserProfile drop index idx_first_last_name;

alter table User add index idx_username (Username);
alter table UserProfile add index idx_first_last_name (LastName(5), FirstName(7));
