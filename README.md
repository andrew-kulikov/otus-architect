# OTUS Highload Architect 

This repository contains practice tasks for OTUS Highload Architect course practice.

The project is Social Network, where users can find friends according to their interests or location.

Functional requirements:
* Users can register
* Users can authorize with username and password
* Passwords stored in secure format (hashed)
* After registration user have to fill personal data (first name, last name, age, city, interests)
* Registered users can see other user profiles

Technical requirements:
* Database: MySQL
* Platform: .NET 5.0
* Language: C#
* No ORM: Dapper (ligtweight SQL object mapper)
* No SQL injections

Implementation notes:
1. [Task 1](docs/task1/README.md) - Prototype. 
2. [Task 2](docs/task2/README.md) - MySQL Indexes.
3. [Task 3](docs/task3/README.md) - Queues and caching.
4. [Task 4](docs/task4/README.md) - MySQL replication.
5. [Task 5](docs/task5/README.md) - In-Memory DBMS (Tarantool).
6. [Task 6](docs/task6/README.md) - Scalable messages system (sharding).
6. [Task 7](docs/task7/README.md) - Realtime news feed update.