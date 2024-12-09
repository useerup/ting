



    Zaphod = (record Name="Zaphod Beeblebrox", Age=42)

    Trillian = (record Name="Tricia McMillan", Age=36)

    SomePerson = (record Name:string, Age:int )

    Persons = { (record Name:string, Age:int) }

    Persons = { list string Name, int Age }




Scrap your boilerplate (https://www.microsoft.com/en-us/research/wp-content/uploads/2003/01/hmap.pdf)


    Company = {. Name:string, Departments:Departments sets .}
    Deptartments = {. Name:string, Manager:Employee, Units:SubInits sets .}
    SubUnits = {| PersonUnit:Employee, DeptartmentUnit:Departments |}
    Employee = {. Person:Person, Salary:decimal .}
    Person = {. Name:string, Address:string .}

    Lists..Sum = 
        (T list items, (T+double) f) => Lists.Sum(items.Map f)  where exists Sets T 


    genCom = Company (. 
        Name = "GenCom"
        Departments = {
            
        }
    .)


---

exists

A record instance:

    Zaphod = (. Name="Zaphod Beeblebrox", Age=42 .)

A nondeterministic record instance

     Person = (. Name:string, Age:int .)

A set of records:

    People = { (. Name:string, Age:int .) }

Shorthand for a set of records
        
    People = {. Name:string, Age:int .}


