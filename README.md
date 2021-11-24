# Academic and administrative management system - MPB

## Table of Contents
* [General Info](#general-information)
* [Requirements](#requirements)
* [Technologies Used](#technologies-used)
* [HTTP API](#http-api)
* [Interfaces](#interfaces)
* [Setup](#setup)
* [Project Status](#project-status)
* [Room for Improvement](#room-for-improvement)
* [Contact](#contact)


## General Information

### **Problem**
<p align="justify">Lack of implementation of information technologies that support the processes and activities of academic and administrative management at I.E.S.T.P "MARIA PARADO DE BELLIDO does not allow making timely decisions that lead to continuous improvement of the quality of service provided to students.</p>

### **Objectives**
<p align="justify">Develop and implement the Academic Administrative Management System MPB, which will provide control mechanisms and information management in order to improve and automate the academic and administrative processes at I.E.S.T.P "MARIA PARADO DE BELLIDO".</p>

- Facilitate the availability and control of access to academic and administrative resources.
- Automate processes related to enrollment, pensions, schedules, grades, academic resources, cash receipts and disbursements, payments and reports.
- Provide tools to administrative staff as a complement to their workflow.

## Requirements


| **N°** |          **User Story**          |
| ------ | -------------------------------- |
|    1   |         Manage students          |
|    2   |         Manage staff             |
|    3   |         Register enrollment      |
|    4   |         Generate enrollment form |
|    5   |         Assign Cashier           |
|    6   |         Register payment         |
|    7   |         Print payment voucher    |
|    8   |         Manage Vault             |
|    9   |         Register student grades  |
|    10  |         Manage users             |



## Technologies Used

- ASP NET MVC - version 5.2.7
- Entity Framework - version 4.7.2
- Razor - version 3.2.7
- Bootstrap - version 3.4.1
- JQuery - version 3.3.1
- Xunit - version  2.4.1
- Sql Server - version 2019


## HTTP API

According to the requirements, the api must contain the following end points.

#### **`Home`**

* `GET /`

#### **`Login`**

* `POST /login`

#### **`User`**

* `GET /user`
* `GET /user/:id`
* `POST /user`
* `PUT /user/:id`
* `DELETE /user/:id`

#### **`Role`**

* `GET /role`
* `GET /role/:id`
* `POST /role`
* `PUT /role/:id`
* `DELETE /role/:id`

#### **`Institution`**

* `GET /instituion`
* `GET /institution/:id`
* `POST /institution`
* `PUT /institution/:id`
* `DELETE /institution/:id`

#### **`Career`**

* `GET /career`
* `GET /career/:id`
* `POST /career`
* `PUT /career/:id`
* `DELETE /career/:id`

#### **`Test`**

* `GET /test`
* `GET /test/:id`
* `POST /test`
* `PUT /test/:id`
* `DELETE /test/:id`

#### **`Alternative`**

* `GET /alternative`
* `GET /alternative/:id`
* `POST /alternative`
* `PUT /alternative/:id`
* `DELETE /alternative/:id`

#### **`Result`**

* `GET /result`
* `GET /result/:id`
* `POST /result`

#### **`Recommendation`**

* `GET /recommendation`
* `GET /recommendation/:id`
* `POST /recommendation`

## Setup

### **Requirements**
* You must have [Entity Framework](https://dotnet.microsoft.com/download/dotnet-framework/net472) 4.7.2 or higher.
* You also must have [GIT](https://git-scm.com/) if you want to contribute to the project.

### **Get the repository locally**
First of all, clone the repository:

```bash
git clone https://github.com/lizelaser/MPB.git
cd <path_to_project>
```

## Usage

Start a development server with IIS Express for development and launch the project on localhost:55756


## Interfaces

### **Login**
![dashboard](./Images/login.png)

### **Home**
![dashboard](./Images/dashboard.png)

### **Enrollment**
![enrollment](./Images/mpb_enrollment.png)

### **Print enrollment**
![print-enrollment](./Images/mpb_print_enrollment.png)

### **Payment**
![payment](./Images/mpb_payment.png)

### **Print payment**
![print-payment](./Images/mpb_print_payment.png)



## Project Status

[![Project Status: Inactive](https://www.repostatus.org/badges/latest/inactive.svg)](https://www.repostatus.org/#inactive)



## Room for Improvement

Room for improvement:
- Publish the project in a cloud environment.
- Support the project on other operating systems using net core technology.

To do:
- Extend the web system by implementing a module for students, allowing them to manage their information, enroll and make payments directly in the system.
- Conduct comprehensive system planning to support the implementation of other processes such as attendance, grades and degrees, remote classes, assessments, library.


## Contact

Lizeth La Serna - [@lizelaser](https://github.com/lizelaser) - lizeth.lasernafelices@gmail.com

Project Link: [https://github.com/lizelaser/MPB](https://github.com/lizelaser/MPB.git)
