﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM_Server__TCP_connection_
{
    public class Employee
    {
        public int Id {get; set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<WorkDay> WorkDays { get; set; }

        public Employee()
        {
        }
        public Employee(string firstname, string lastname)
        {
            FirstName = firstname;
            LastName = lastname;
      
        }  

        public static string GetStringList()       // Возвращаем список сотрудников
        {
            StringBuilder empList = new StringBuilder();

            using (var context = new MyDbContext())
            {

                foreach (var emp in context.Employees)
                {
                    empList.Append($"\nId{emp.Id} \t{emp.FirstName} {emp.LastName}");
                }
            }

            var result = empList.ToString();
            return result;
        }

        public override string ToString()
        {
            if (Id < 10)
            {
                return $"Id{Id}      {FirstName} {LastName}";
            }
            else
            {
                return $"Id{Id}     {FirstName} {LastName}";
            }
            
        }

        public static int[] GetAllPossibleIds()
        {
            int[] result;
            using (var context = new MyDbContext())
            {
                result = context.Employees.Select(x => x.Id).ToArray();
            }
            return result;
        }

        public static Employee GetById(int id)    // Возвращает сотрудника по Id
        {
            Employee emp = new Employee();
            
            using (var context = new MyDbContext())
            {
                emp = context.Employees.Single(x => x.Id == id);
            }
            return emp;
        }


        public static string AddFullWorkDay(ClientObject client)  // Добавляем новый полный рабочий день                   
        {
            var emp = client.Answers[1].EmployeeAnswer;
            var workdate = client.Answers[2].DatetimeAnswer;

            string result = "";

                if (GetInfoWorkHoursAtDate(client) == 0)
                {
                    using (var context = new MyDbContext())
                    {
                    context.WorkDays.Add(new WorkDay(workdate, 8.0, emp.Id));
                    context.SaveChanges();
                    }

                    result += $"\nТеперь количество часов отработанных сотрудником {emp.FirstName} {emp.LastName} за {workdate:d} составляет 8 рабочих часов ";
                    FileHandler.AddLog($"\tПользователь {client.userName} добавил сотруднику {emp.FirstName} {emp.LastName} 8 рабочих часов за {workdate:d} ");
                }
                else                                                // Если рабочий день уже существует 
                {
                    result += $"\nУказано что {emp.FirstName} {emp.LastName} за {workdate:d} отработал {GetInfoWorkHoursAtDate(client)} рабочих часов\nДля внесение изменений в существующие данные воспользуйтесь пунктом 7 'Отредактировать информацию'";
                    FileHandler.AddLog($"\tПользователь {client.userName} не добавил сотруднику {emp.FirstName} {emp.LastName} новый полный рабочий день, поскольку информация про работу сотрудника в эту дату уже внесена.");
                }

            return result;
        }

        public static string AddPartWorkDay(ClientObject client)    
        {
            var emp = client.Answers[1].EmployeeAnswer;
            var workdate = client.Answers[2].DatetimeAnswer;
            var hours = client.Answers[3].DoubleAnswer;

            string result = "";

            if (GetInfoWorkHoursAtDate(client) == 0)          // Добавляем новый неполный рабочий день
            {
                using (var context = new MyDbContext())
                {
                    context.WorkDays.Add(new WorkDay(workdate, hours, emp.Id));
                    context.SaveChanges();
                }

                result += $"\nТеперь количество часов отработанных сотрудником {emp.FirstName} {emp.LastName} за {workdate:d} составляет {hours} рабочих часов ";
                FileHandler.AddLog($"\tПользователь {client.userName} добавил сотруднику {emp.FirstName} {emp.LastName} {hours} рабочих часов за {workdate:d} ");
            }
            else                                                // Если рабочий день уже существует 
            {
                result += $"\nУказано что {emp.FirstName} {emp.LastName} за {workdate:d} отработал {GetInfoWorkHoursAtDate(client)} рабочих часов\nДля внесение изменений в существующие данные воспользуйтесь пунктом 7 'Отредактировать информацию'";
                FileHandler.AddLog($"\tПользователь {client.userName} не добавил сотруднику {emp.FirstName} {emp.LastName} новый неполный рабочий день, поскольку информация про работу сотрудника в эту дату уже внесена.");
            }

            return result;
        }

        public static double GetInfoWorkHoursAtDate(ClientObject client) // Возвращает количество рабочих часов сотрудника в определенную дату
        {
            var emp = client.Answers[1].EmployeeAnswer;
            var workdate = client.Answers[2].DatetimeAnswer;

            using (var context = new MyDbContext())
            {
                var workday = context.WorkDays.Where(x => x.EmployeeId == emp.Id).Where(x => x.WorkDate.Equals(workdate));

                if (workday.Count() > 0)
                {
                    double result;
                    try
                    {
                        result = workday.Single().WorkHours;
                        return result;
                    }
                    catch (Exception ex)
                    {                    
                        FileHandler.AddLog($"Ошибка чтения Сервера SQL: Более одной записи о рабочей дате {workdate:d} у сотрудника {emp.Id}: {DateTime.Now} | {ex.Message}");
                    }               
                }

                return 0;

            }
                
        }

        public static string GetInfoWorkHoursAtDateAsString(ClientObject client) // Возвращает количество рабочих часов сотрудника в определенную дату в формает строки
        {
            var emp = client.Answers[1].EmployeeAnswer;
            var workdate = client.Answers[2].DatetimeAnswer;

            double hours = 0;

            using (var context = new MyDbContext())
            {
                var workday = context.WorkDays.Where(x => x.EmployeeId == emp.Id).Where(x => x.WorkDate.Equals(workdate));

                if (workday.Count() > 0)
                {
                    try
                    {
                        hours = workday.Single().WorkHours;
                    }
                    catch (Exception ex)
                    {
                        FileHandler.AddLog($"Ошибка чтения Сервера SQL: Более одной записи о рабочей дате {workdate:d} у сотрудника {emp.Id}: {DateTime.Now} | {ex.Message}");
                    }
                }
            }

            FileHandler.AddLog($"\tПользователь {client.userName} запросил информацию про количество отработанных часов {emp.FirstName} {emp.LastName} за {workdate:d}");

            if (hours == 0)
            {
                return $"\nСотрудник {emp.FirstName} {emp.LastName} не работал {workdate:d}";
            }
            else
            {
                return $"\nСотрудник {emp.FirstName} {emp.LastName} отработал за {workdate:d} {hours} часов.";
            }
            
        }

        public static string GetInfoWorkHoursPerPeriod(ClientObject client)    // Возвращает количество рабочих часов сотрудника за период
        {
            var emp = client.Answers[1].EmployeeAnswer;
            var firstdate = client.Answers[2].DatetimeAnswer;
            var seconddate = client.Answers[3].DatetimeAnswer;

            if (firstdate > seconddate)
            {
                (firstdate, seconddate) = (seconddate, firstdate);
            }

            double sum = 0;

            using (var context = new MyDbContext())
            {
                var workday = context.WorkDays.Where(x => x.EmployeeId == emp.Id).Where(x => x.WorkDate > firstdate).Where(x => x.WorkDate < seconddate);

                if (workday.Count() > 0)
                {
                    try
                    {
                        sum = workday.Select(x => x.WorkHours).Sum();
                    }
                    catch (Exception ex)
                    {
                        FileHandler.AddLog($"Ошибка чтения Сервера SQL при попытке подсчета суммы проработанных часов. {emp.Id}: {DateTime.Now} | {ex.Message}");
                    }
                }
            }

            FileHandler.AddLog($"\tПользователь {client.userName} запросил информацию про количество отработанных часов {emp.FirstName} {emp.LastName} за период с {firstdate:d} по {seconddate:d}");

            if (sum == 0)
            {
                return $"\nСотрудник {emp.FirstName} {emp.LastName} не работал с {firstdate:d} по {seconddate:d}";
            }
            else
            {
                return $"\nСотрудник {emp.FirstName} {emp.LastName} отработал с {firstdate:d} по {seconddate:d} {sum} часов.";
            }
        }

        public static string GetInfoWorkHoursAllTime(ClientObject client)            // Возвращает количество рабочих часов сотрудника за все время
        {
            var emp = client.Answers[1].EmployeeAnswer;
            double sum = 0;

            using (var context = new MyDbContext())
            {
                var workday = context.WorkDays.Where(x => x.EmployeeId == emp.Id);

                if (workday.Count() > 0)
                {
                    try
                    {
                        sum = workday.Select(x => x.WorkHours).Sum();
                    }
                    catch (Exception ex)
                    {
                        FileHandler.AddLog($"Ошибка чтения Сервера SQL при попытке подсчета суммы проработанных часов. {emp.Id}: {DateTime.Now} | {ex.Message}");
                    }
                }
            }

            FileHandler.AddLog($"\tПользователь {client.userName} запросил информацию про количество отработанных часов {emp.FirstName} {emp.LastName} за весь период");

            if (sum == 0)
            {
                return $"\nСотрудник {emp.FirstName} {emp.LastName} не отработал ни одного часа.";
            }
            else
            {
                return $"\nСотрудник {emp.FirstName} {emp.LastName} отработал за все время {sum} часов.";
            }

        }


        public static string GetListOfEmployeesAndWorkHours(ClientObject client)       // Выводим список сотрудников и отработанных часов
        {
            var line = new StringBuilder();
            line.Append("\nСписок сотрудников и рабочих часов:\n");

            using (var context = new MyDbContext())
            {                 
                var employees = context.Employees.ToList();

                foreach (var emp in employees)
                {
                    var sum = emp.WorkDays.Select(x => x.WorkHours).Sum();

                    line.Append($"\n{emp}");

                    int dif = 35 - ($"\n{emp}").Length;
                    if (dif > 0) { line.Append(' ', dif);  };
                    // Что бы все было в одну линию 

                    line.Append($"отработал {sum} часов.");
                }
            }

            FileHandler.AddLog($"\tПользователь {client.userName} запросил информацию про количество отработанных часов всех сотрудников за все время работы");

            if (line.Length > 0)
            {
                return line.ToString();
            }
            else
            {
                return $"\nВ базе данных пока что нет сотрудников с отработанными часами.";
            }
        }


        public static string EditInfoAboutWorkDay(ClientObject client)
        {
            var emp = client.Answers[1].EmployeeAnswer;
            var workdate = client.Answers[2].DatetimeAnswer;
            var hours = client.Answers[3].DoubleAnswer;
            double oldHours = 0;

            using (var context = new MyDbContext())
            {
                var workdays = context.WorkDays.Where(x => x.EmployeeId == emp.Id).Where(x => x.WorkDate.Equals(workdate));

                if (workdays.Count() > 0)
                {   // Если рабочий день существует
                    try
                    {
                        oldHours = workdays.First().WorkHours;
                        workdays.First().WorkHours = hours;
                    }
                    catch (Exception ex)
                    {
                        FileHandler.AddLog($"Ошибка чтения Сервера SQL при попытке передать новое значение отработанных часов сотрудника. {emp.Id}: {DateTime.Now} | {ex.Message}");
                    }
                }
                else
                {   // Если рабочего дня не существует
                    context.WorkDays.Add(new WorkDay(workdate, hours, emp.Id));
                }

                context.SaveChanges();
            }

            FileHandler.AddLog($"\tПользователь {client.userName} изменил информацию про количество отработанных часов {emp.FirstName} {emp.LastName} за {workdate:d}. Было {oldHours} часов, стало {hours} часов");
            return $"Информация отредактирована. Зафиксировано что {emp.FirstName} {emp.LastName} за {workdate:d} отработал {hours} рабочих часов";

        }

        public static string ToFireEmployee(ClientObject client)
        {
            var emp = client.Answers[1].EmployeeAnswer;
            var isSure = client.Answers[2].ConsolekeyAnswer;

            if (isSure.Equals(ConsoleKey.Y))
            {
                using (var context = new MyDbContext())
                {
                    var empl = context.Employees.First(x => x.Id == emp.Id);
                    context.Employees.Remove(empl);
                    context.SaveChanges();
                }

                FileHandler.AddLog($"\tПользователь {client.userName} уволил {emp.FirstName} {emp.LastName}");
                return $"Сотрудник {emp.FirstName} {emp.LastName} уволен";
            }
            else if (isSure.Equals(ConsoleKey.N))
            {
                FileHandler.AddLog($"\tПользователь {client.userName} отменил процедуру увольнения {emp.FirstName} {emp.LastName}");
                return $"Сотрудник {emp.FirstName} {emp.LastName} не уволен. Процедура отменена.";
            }
            return "";
        }

        public static void ToHireEmployee(ClientObject client)
        {
            var firstname = client.Answers[1].StringAnswer;
            var lastname = client.Answers[2].StringAnswer;

            using (var context = new MyDbContext())
            {
                context.Employees.Add(new Employee(firstname, lastname));
                context.SaveChanges();
            }
        }
    }
}
