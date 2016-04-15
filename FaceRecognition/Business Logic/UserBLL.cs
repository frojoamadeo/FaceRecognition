using FaceRecognition.DAL;
using FaceRecognition.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FaceRecognition.Business_Logic
{
    public class UserBLL
    {
        GenericUnitOfWork unitOfWork;

        public UserBLL() 
        {
            unitOfWork = new GenericUnitOfWork();
            unitOfWork.SaveChanges();
        }

        public bool isValid(string _email, string _passwordPlane) 
        {
            string _password = Helpers.SHA256.Encode(_passwordPlane);
            GenericRepository<Employee> employeeRepo = unitOfWork.GetRepoInstance<Employee>();

            try
            {
                Employee employee = null;
                employee = (employeeRepo.GetAllRecords().Where<Employee>(e => e.email == _email && e.password == _password)).First<Employee>();
                if (employee != null)
                {
                    employeeRepo.Update(employee);
                    unitOfWork.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                } 
            }
            catch
            {
                return false;
            }
        }

        public bool addEmployee(Employee employee)
        {


            if (!isValid(employee.email, employee.password))
            {               
                try
                {
                    employee.password = Helpers.SHA256.Encode(employee.password);
                    GenericRepository<Employee> employeeRepo = unitOfWork.GetRepoInstance<Employee>();
                    employeeRepo.Add(employee);
                    unitOfWork.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}