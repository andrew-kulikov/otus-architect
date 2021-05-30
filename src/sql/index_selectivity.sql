select count(*) as cnt, left(LastName, 1) as prefix 
from UserProfile 
group by prefix order by cnt desc;

select count(DISTINCT LastName)/count(LastName) as originalSelectivity, 
count(DISTINCT left(LastName,1))/count(LastName) as prefix1, 
count(DISTINCT left(LastName,2))/count(LastName) as prefix2, 
count(DISTINCT left(LastName,3))/count(LastName) as prefix3, 
count(DISTINCT left(LastName,4))/count(LastName) as prefix4, 
count(DISTINCT left(LastName,5))/count(LastName) as prefix5, 
count(DISTINCT left(LastName,6))/count(LastName) as prefix6, 
count(DISTINCT left(LastName,7))/count(LastName) as prefix7, 
count(DISTINCT left(LastName,8))/count(LastName) as prefix8, 
count(DISTINCT left(LastName,9))/count(LastName) as prefix9, 
count(DISTINCT left(LastName,10))/count(LastName) as prefix10
from UserProfile;

select count(DISTINCT FirstName)/count(FirstName) as originalSelectivity, 
count(DISTINCT left(FirstName,1))/count(FirstName) as prefix1, 
count(DISTINCT left(FirstName,2))/count(FirstName) as prefix2, 
count(DISTINCT left(FirstName,3))/count(FirstName) as prefix3, 
count(DISTINCT left(FirstName,4))/count(FirstName) as prefix4, 
count(DISTINCT left(FirstName,5))/count(FirstName) as prefix5, 
count(DISTINCT left(FirstName,6))/count(FirstName) as prefix6, 
count(DISTINCT left(FirstName,7))/count(FirstName) as prefix7, 
count(DISTINCT left(FirstName,8))/count(FirstName) as prefix8, 
count(DISTINCT left(FirstName,9))/count(FirstName) as prefix9, 
count(DISTINCT left(FirstName,10))/count(FirstName) as prefix10
from UserProfile;

explain analyze
select * from UserProfile where FirstName like 'a%' and LastName like 'a%';
