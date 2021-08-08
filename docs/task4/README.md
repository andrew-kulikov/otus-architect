## Задание
---



## Результат
---

Сначала проверяем на тестовой среде. 

Настраиваем асинхронную репликацию:

1. Разворачиваем 2 контейнера в докере с лимитами по производительности для проверки нагрузки. [Скрипт](../../scripts/mysql-async-replication.yml)
2. Проверяем, что mysql запущен
  ```bash
  > mysql -u root -p
  ```
3. Задаем server id на мастере и слейве. 
  ```mysql
  mysql> SET GLOBAL server_id = 1;` `SET GLOBAL server_id = 2;
  ```
4. Создаем юзера для репликации на мастере.  
  ```mysql
  mysql> create user 'repl' identified by '123';
  mysql> grant replication slave on *.* to repl; 
  ```
5. Для синхронизации необходимо перенести все данные с мастера на слейв. Для этого включаем лок на чтение, и делаем дамп базы. 
  ```mysql
  mysql> FLUSH TABLES WITH READ LOCK;
  mysql> SHOW MASTER STATUS;
  ```
6. Сделаем дамп базы
  ```bash
  > mysqldump --user="root" --password="" --all-databases --master-data > dbdump.db
  ```
7. Перенесем дамп между контейнерами (по умолчанию докер не поддерживает перенос файлов между контейнерами) 
  ```bash
  > docker cp mysql_master:/dbdump.db D:\temp\dbdump.db
  > docker cp D:\temp\dbdump.db mysql_replica-1:/dbdump.db
  ```
8. Поднимаем дамп на слейве: 
  ```bash
  > mysql -u root -p < dbdump.db
  ```
9. Задаем мастера для слейва 
  ```mysql
  mysql> CHANGE MASTER TO
        MASTER_HOST='mysql_master',
        MASTER_USER='repl',
        MASTER_PASSWORD='123',
        MASTER_LOG_FILE='binlog.000002',
        MASTER_LOG_POS=1355;
  ```
10. Разблокируем таблицы на мастере
  ```mysql
  mysql> UNLOCK TABLES;
  ```
11. Перезагружаем слейва
  ```mysql
  mysql> stop slave;
  mysql> start slave;
  ```
12. Фиксим ошибку авторизации (в проде нужно настраивать сертификаты, для тестов можно отключить):
  ```mysql 
  mysql> alter user 'repl' IDENTIFIED WITH mysql_native_password BY '123';
  ```
  
  
  
### Использованные источники

* https://dev.mysql.com/doc/refman/8.0/en/replication.html
* https://stackoverflow.com/questions/49194719/authentication-plugin-caching-sha2-password-cannot-be-loaded
* https://dev.mysql.com/doc/mysql-shell/8.0/en/mysql-innodb-cluster.html
* https://dev.mysql.com/doc/refman/8.0/en/mysqldump.html