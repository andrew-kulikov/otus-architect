## Задание

Необходимо написать систему диалогов между пользователями. Обеспечить горизонтальное масштабирование хранилищ на запись с помощью шардинга. Предусмотреть:  
* Возможность решардинга
* “Эффект Леди Гаги” (один пользователь пишет сильно больше среднего)

Требования: Верно выбран ключ шардирования с учетом "эффекта Леди Гаги" В отчете описан процесс решардинга без даунтайма

## Результат

<br/>

### Проектирование

В результате выполнения задачи была спроектирована следующая структура сервиса личных сообщений:

<img src="messages.jpg" alt="messages" width="1100"/>  

<br/>

### Структура базы данных  

```SQL
CREATE TABLE IF NOT EXISTS  `Chat` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `IsPersonal` BOOLEAN NOT NULL,
    CONSTRAINT `PK_Chat` PRIMARY KEY (`Id`)) ENGINE = INNODB;


CREATE TABLE IF NOT EXISTS  `ChatMember` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `ChatId` BIGINT NOT NULL,
    `UserId` BIGINT NOT NULL,
    CONSTRAINT `PK_ChatMember` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ChatMember_UserId_UserProfile_Id` 
        FOREIGN KEY (`UserId`) 
        REFERENCES `UserProfile` (`UserId`)) ENGINE = INNODB;


CREATE TABLE IF NOT EXISTS  `ChatMessage` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `ChatId` BIGINT NOT NULL,  
    `SenderId` BIGINT NOT NULL,  
    `ChatLocalId` INTEGER NOT NULL, 
    `Text` TEXT NOT NULL, 
    `Created` DATETIME NOT NULL,
    `Updated` DATETIME NOT NULL,
    `IsDeleted` BOOLEAN NOT NULL,
    CONSTRAINT `PK_Message` PRIMARY KEY (`Id`)) ENGINE = INNODB;
    
alter table ChatMessage add index idx_chatId (ChatId);
```
При этом таблицы Chat и ChatMember могут храниться в основной базе, так как содержат метаинформацию и на запросы к ним не приходится большая часть нагрузки. Каждое сообщение имеет глобальный идентификатор (Id) - уникальный в рамках шарда. Также каждое сообщение имеет уникальный идентификатор в рамках чата - ChatLocalId, который может быть использован для удобства операций внутри одного чата.

<br/>

На основе анализа известных архитектур таких ситем, как Facebook Messenger, VK, Pinterest было принято решение, что все сообщения чата должны находиться на одном шарде для облегчения простоты доступа к данным, а также возможности будущего решардинга. Исходя из этого получаем следующие алгоритмы загрузки чата и записи сообщения:

### Алгоритм публикации сообщения 

1. Запрос попадает в систему через API
2. Сервис сообщений идет в Redis за текущим RPM пользователя.
3. На основе этой информации определяется "Lady Gaga" - активный пользователь, который пишет много сообщений. Его сообщение помечается соответствующей меткой
4. Сообщение отправляется в RabbitMQ в соответсвующую очередь. Здесь можно делать троттлинг активных пользователей, чтобы не заваливать базу запросами.
5. Сообщение направлятся соответствующему consumer'у
6. Consumer на основе сконфигурированных промежутков chat_id (range sharding) определяет id шарда, на котором находится данный чат.  
7. Производится запись в соответствующий шард.

<br/>

### Алгоритм загрузки сообщений чата

1. Запрос попадает в систему через API
2. Сервис сообщений на основе сконфигурированных промежутков chat_id (range sharding) определяет id шарда, на котором находится данный чат.  
3. Производится чтение из соответствующего шарда.

<br/>

### Алгоритм решардинга

Алгоритм решардинга похож на упрощенную версию алгоритма решардинга / миграции Facebook Messenger. ProxySQL не поддерживает Consistent Hashing, для этого нужно либо реализовывать свои инструменты, либо пользоваться более сложной схемой решардинга.

1. Добавляем новый шард
2. Настраиваем репликацию между новым шардом и тем, с которого хотим перенести данные
3. Конфигурируем ProxySQL и направляем трафик на оба шарда
4. Меняем конфигурацию id ренжей для шардов 

<br/>

### Настройка ProxySQL

Для тестовой среды было настроено 2 шарда MySQL и ProxySQL:
```yaml
messages-db-1:
  image: mysql:8.0
  container_name: messages-db-1
  restart: always
  environment:
    MYSQL_DATABASE: 'db'
    MYSQL_USER: 'zukk'
    MYSQL_PASSWORD: 'zukk'
    MYSQL_ROOT_PASSWORD: 'admin'
  ports:
    - '3307:3306'
  expose:
    - '3306'
  volumes:
    - messages-db-1:/var/lib/mysql
messages-db-2:
  image: mysql:8.0
  container_name: messages-db-2
  restart: always
  environment:
    MYSQL_DATABASE: 'db'
    MYSQL_USER: 'zukk'
    MYSQL_PASSWORD: 'zukk'
    MYSQL_ROOT_PASSWORD: 'admin'
  ports:
    - '3308:3306'
  expose:
    - '3306'
  volumes:
    - messages-db-2:/var/lib/mysql
proxysql:
  build:
    context: proxysql
    dockerfile: Dockerfile
  container_name: proxysql
  volumes:
    - proxysql-data:/var/lib/proxysql
  ports:
    # Mysql Client Port
    - "6033:6033"
    # Mysql Admin Port
    - "6032:6032"
```

Конфигурация серверов для ProxySQL:
```conf
mysql_servers =
(
    {
        address="messages-db-1"
        port=3306
        hostgroup=0
        max_connections=200
    },
    {
        address="messages-db-2"
        port=3306
        hostgroup=1
        max_connections=200
    }
)
```

Конфигурация провил роутинга для ProxySQL:
```conf
mysql_query_rules =
(
    {
        rule_id=1
        username=root
        active=1
        match_pattern="\/\*\s*shard0000\s*\*."
        destination_hostgroup=0
        apply=1
    },
    {
        rule_id=2
        username=root
        active=1
        match_pattern="\/\*\s*shard0001\s*\*."
        destination_hostgroup=1
        apply=1
    },
)
```

Пример запросов:
```SQL
select /* shard0001 */ * from ChatMessage 
where ChatId = 3 
order by ChatLocalId desc 
limit 10 offset 0;

---------

insert /* shard0001 */ into ChatMessage (ChatId, SenderId, ChatLocalId, Text, Created, Updated, IsDeleted)
values (@ChatId, @SenderId, @ChatLocalId, @Text, @Created, @Updated, @IsDeleted)
```

<br/>

### Точки расширения

Также были предусмотрены следующие точки расширения, которые могут быть исопльзованы при росте нагрузки на систему:
1. API Gateway для горизонтального масштабирования сервисов сообщений
2. Запись может масштабироваться горизонтально благодаря брокеру сообщений
3. База может масштабироваться горизонтально благодаря ProxySQL - добавляем инстансы, указываем в конфигурации.
4. В слушае добавления чат-ботов, которые будут слать много сообщений, их чаты имеет смысл хранить на отдельных шардах, и обслуживать отдельными consumer'ами, чтобы это никак не влияло на производительность системы для обычных пользователей.

### Использованные источники

* https://medium.com/pinterest-engineering/sharding-pinterest-how-we-scaled-our-mysql-fleet-3f341e96ca6f
* https://engineering.fb.com/2018/06/26/core-data/migrating-messenger-storage-to-optimize-performance/
* https://proxysql.com/documentation/
* https://www.percona.com/blog/2016/08/30/mysql-sharding-with-proxysql/
* http://www.tusacentral.com/joomla/index.php/mysql-blogs/193-how-proxysql-deal-with-schema-and-schemaname
* https://proxysql.com/blog/new-schemaname-routing-algorithm/
* https://proxysql.com/documentation/configuration-file/