Practical assessment for Gadu Abdulrasheed
Email: [abdulgadu@gmail.com]

Overview

This is a simplified healthcare management system designed to manage:

- Patient records
- Appointments
- Invoices
- Payments

Sample data is included and will seed automatically when the project is first run, allowing you to begin testing immediately.

Well detailled comments added for good understanding of the codebase

Note: Kindly update the connection string before running the project.


Login Credentials

Use the following test accounts:

Admin

Username: admin
Password: admin123
Role: Admin

Superadmin

Username: superadmin
Password: superadmin123
Role: Superadmin


Implemented Features

The following assessment tasks were fully implemented:

Records Landing Screen
Create Invoice – Wallet Mode
Pay Invoice – Wallet
Menu Actions (Create Invoice / Pay Invoice)

Pagination / Filtering Approach

Client-side filtering and pagination were used intentionally to ensure flexibility if a separate frontend developer will later consume the backend API.



Patient Status Flow:

Processing


The patient may be at the front desk for:

 - Registration
 - Appointment scheduling
 - Invoice creation
 - Payment processing (before payment is completed)

Awaiting Vitals

Set automatically after a successful payment (full or partial).
Indicates that the patient has completed billing and is ready for the next clinical step (vitals collection).
