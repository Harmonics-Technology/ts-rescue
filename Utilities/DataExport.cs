using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using TimesheetBE.Models.AppModels;
//using System.IO;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Utilities.Abstrctions;

namespace TimesheetBE.Utilities
{
    public class DataExport : IDataExport
    {
        public byte[] ExportAdminUsers(RecordsToDownload recordType, List<User> record, List<string> rowHeaders)
        {
            using(var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(recordType.ToString());
                var currentRow = 1;
                switch (recordType)
                {
                    case RecordsToDownload.AdminUsers:
                        int rowIndex = 1;
                        foreach (var rowHead in rowHeaders)
                        {
                            worksheet.Cell(currentRow, rowIndex).Value = rowHead;
                            rowIndex++;
                        }

                        foreach(var user in record)
                        {
                            currentRow++;
                            int rowIndexRecord = 1;
                            foreach (var rowHead in rowHeaders)
                            {
                                worksheet.Cell(currentRow, rowIndexRecord).Value = rowHead == "Name" ? 
                                    user.FullName : rowHead == "Email" ? 
                                    user.Email : rowHead == "Role" ? 
                                    user.Role : rowHead == "Status" ? 
                                    (user.IsActive ? "Active" : "Inactive") : "No Record";
                                rowIndexRecord++;
                            }
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            var content = stream.ToArray();
                            return content;
                        }
                        break;
                    case RecordsToDownload.TeamMembers:
                        rowIndex = 1;
                        foreach (var rowHead in rowHeaders)
                        {
                            worksheet.Cell(currentRow, rowIndex).Value = rowHead;
                            rowIndex++;
                        }

                        foreach (var user in record)
                        {
                            currentRow++;
                            int rowIndexRecord = 1;
                            foreach (var rowHead in rowHeaders)
                            {
                                worksheet.Cell(currentRow, rowIndexRecord).Value = rowHead == "Name" ?
                                    user.FullName : rowHead == "Job Title" ?
                                    user.EmployeeInformation.JobTitle : rowHead == "Client Name" ?
                                    user.EmployeeInformation.Client.OrganizationName : rowHead == "Payroll Type" ?
                                    (user.EmployeeInformation.PayRollTypeId == 1 ? "Onshore" : "Offshore") : rowHead == "Role" ?
                                    user.Role : rowHead == "Status" ?
                                    (user.IsActive ? "Active" : "Inactive") : "No Record";
                                rowIndexRecord++;
                            }
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            var content = stream.ToArray();
                            return content;
                        }
                        break;
                    case RecordsToDownload.Supervisors:
                        goto case RecordsToDownload.AdminUsers;
                    case RecordsToDownload.Client:
                        rowIndex = 1;
                        foreach (var rowHead in rowHeaders)
                        {
                            worksheet.Cell(currentRow, rowIndex).Value = rowHead;
                            rowIndex++;
                        }

                        foreach (var user in record)
                        {
                            currentRow++;
                            int rowIndexRecord = 1;
                            foreach (var rowHead in rowHeaders)
                            {
                                worksheet.Cell(currentRow, rowIndexRecord).Value = rowHead == "Name" ?
                                    user.FullName : rowHead == "Email" ?
                                    user.Email : rowHead == "Role" ?
                                    user.Role : rowHead == "Phone" ?
                                    user.PhoneNumber : rowHead == "Invoice Schedule" ?
                                    user.InvoiceGenerationFrequency : rowHead == "Status" ?
                                    (user.IsActive ? "Active" : "Inactive") : "No Record";
                                rowIndexRecord++;
                            }
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            var content = stream.ToArray();
                            return content;
                        }
                        break;
                    case RecordsToDownload.PaymentPartner:
                        goto case RecordsToDownload.AdminUsers;
                    case RecordsToDownload.PayrollManagers:
                        goto case RecordsToDownload.AdminUsers;
                    case RecordsToDownload.Admin:
                        goto case RecordsToDownload.AdminUsers;
                    case RecordsToDownload.ClientTeamMembers:
                        goto case RecordsToDownload.AdminUsers;
                    case RecordsToDownload.ClientSupervisors:
                        goto case RecordsToDownload.AdminUsers;
                    case RecordsToDownload.Supervisees:
                        goto case RecordsToDownload.TeamMembers;
                    case RecordsToDownload.PaymentPartnerTeamMembers:
                        goto case RecordsToDownload.TeamMembers;
                    default:
                        return null;
                        break;

                }
                

            }
           
        }

        public byte[] ExportInvoiceRecords(InvoiceRecord recordType, List<Invoice> record, List<string> rowHeaders)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(recordType.ToString());
                var currentRow = 1;
                switch (recordType)
                {
                    case InvoiceRecord.PendingPayrolls:
                        int rowIndex = 1;
                        foreach (var rowHead in rowHeaders)
                        {
                            worksheet.Cell(currentRow, rowIndex).Value = rowHead;
                            rowIndex++;
                        }
                        foreach (var invoice in record)
                        {
                            currentRow++;
                            int rowIndexRecord = 1;
                            foreach (var rowHead in rowHeaders)
                            {
                                worksheet.Cell(currentRow, rowIndexRecord).Value = rowHead == "Payroll Group" ?
                                    //(invoice.EmployeeInformation.Client.OrganizationName == 1 ? "Proinsight" : "Olade") : rowHead == "Name" ?
                                    invoice.EmployeeInformation.Client.OrganizationName : rowHead == "Name" ?
                                    invoice.CreatedByUser.FullName : rowHead == "Created On" ?
                                    invoice.DateCreated.Date.ToString() : rowHead == "Start Date" ?
                                    invoice.StartDate.Date.ToString() : rowHead == "End Date" ?
                                    invoice.EndDate.Date.ToString() : rowHead == "Status" ?
                                    invoice.Status.Name : "No Record";
                                rowIndexRecord++;
                            }
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            var content = stream.ToArray();
                            return content;
                        }
                        break;
                    case InvoiceRecord.ProcessedPayrolls:
                        goto case InvoiceRecord.PendingPayrolls;
                    case InvoiceRecord.PendingInvoices:
                        rowIndex = 1;
                        foreach (var rowHead in rowHeaders)
                        {
                            worksheet.Cell(currentRow, rowIndex).Value = rowHead;
                            rowIndex++;
                        }

                        foreach (var invoice in record)
                        {
                            currentRow++;
                            int rowIndexRecord = 1;
                            foreach (var rowHead in rowHeaders)
                            {
                                worksheet.Cell(currentRow, rowIndexRecord).Value = rowHead == "Invoice No" ?
                                    invoice.InvoiceReference : rowHead == "Name" ?
                                    invoice.CreatedByUser.FullName : rowHead == "Created On" ?
                                    invoice.DateCreated.Date.ToString() : rowHead == "Start Date" ?
                                    invoice.StartDate.Date.ToString() : rowHead == "End Date" ?
                                    invoice.EndDate.Date.ToString() : rowHead == "Status" ?
                                    invoice.Status.Name : "No Record";
                                rowIndexRecord++;
                            }
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            var content = stream.ToArray();
                            return content;
                        }
                        break;
                    case InvoiceRecord.ProcessedInvoices:
                        goto case InvoiceRecord.PendingInvoices;
                    case InvoiceRecord.PaymentPartnerInvoices:
                        rowIndex = 1;
                        foreach (var rowHead in rowHeaders)
                        {
                            worksheet.Cell(currentRow, rowIndex).Value = rowHead;
                            rowIndex++;
                        }

                        foreach (var invoice in record)
                        {
                            currentRow++;
                            int rowIndexRecord = 1;
                            foreach (var rowHead in rowHeaders)
                            {
                                worksheet.Cell(currentRow, rowIndexRecord).Value = rowHead == "Invoice No" ?
                                    invoice.InvoiceReference : rowHead == "Name" ?
                                    //(invoice.PayrollGroupId == 1 ? "Proinsight" : "Olade") : rowHead == "Created On" ?
                                    invoice.Client.OrganizationName : rowHead == "Created On" ?
                                    invoice.DateCreated.Date.ToString() : rowHead == "Amount($)" ?
                                    Math.Round(invoice.TotalAmount, 2) : rowHead == "Amount(₦)" ?
                                    $"{Math.Round(invoice.TotalAmount * Convert.ToDouble(invoice.Rate), 2)}" : rowHead == "Status" ?
                                    invoice.Status.Name : "No Record";
                                rowIndexRecord++;
                            }
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            var content = stream.ToArray();
                            return content;
                        }
                        break;
                    case InvoiceRecord.ClientInvoices:
                        goto case InvoiceRecord.PendingInvoices;
                    default:
                        return null;
                        break;

                }


            }
        }

        public byte[] ExportExpenseRecords(ExpenseRecordsToDownload recordType, List<Expense> record, List<string> rowHeaders)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(recordType.ToString());
                var currentRow = 1;
                switch (recordType)
                {
                    case ExpenseRecordsToDownload.ReviwedExpenses:
                        int rowIndex = 1;
                        foreach (var rowHead in rowHeaders)
                        {
                            worksheet.Cell(currentRow, rowIndex).Value = rowHead;
                            rowIndex++;
                        }
                        foreach (var expense in record)
                        {
                            currentRow++;
                            int rowIndexRecord = 1;
                            foreach (var rowHead in rowHeaders)
                            {
                                worksheet.Cell(currentRow, rowIndexRecord).Value = rowHead == "Name" ?
                                    expense.TeamMember.FullName : rowHead == "Description" ?
                                    expense.Description : rowHead == "Expense Type" ?
                                    expense.ExpenseType.Name : rowHead == "Expense Date" ?
                                    expense.ExpenseDate?.Date.ToString() : rowHead == "Created On" ?
                                    expense.DateCreated.Date.ToString() : rowHead == "Amount" ?
                                    Math.Round(expense.Amount, 2) : rowHead == "Status" ?
                                    expense.Status.Name : "No Record";
                                rowIndexRecord++;
                            }
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            var content = stream.ToArray();
                            return content;
                        }
                        break;
                    case ExpenseRecordsToDownload.ApprovedExpenses:
                        rowIndex = 1;
                        foreach (var rowHead in rowHeaders)
                        {
                            worksheet.Cell(currentRow, rowIndex).Value = rowHead;
                            rowIndex++;
                        }

                        foreach (var expense in record)
                        {
                            currentRow++;
                            int rowIndexRecord = 1;
                            foreach (var rowHead in rowHeaders)
                            {
                                worksheet.Cell(currentRow, rowIndexRecord).Value = rowHead == "Name" ?
                                    expense.TeamMember.FullName : rowHead == "Description" ?
                                    expense.Description : rowHead == "Expense Type" ?
                                    expense.ExpenseType.Name : rowHead == "Expense Date" ?
                                    expense.ExpenseDate?.Date.ToString() : rowHead == "Currency" ?
                                    expense.Currency : rowHead == "Amount" ?
                                    Math.Round(expense.Amount, 2) : rowHead == "Status" ?
                                    expense.Status.Name : "No Record";
                                rowIndexRecord++;
                            }
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            var content = stream.ToArray();
                            return content;
                        }
                        break;
                    default:
                        return null;
                        break;

                }


            }
        }

        public byte[] ExportPayslipRecords(PayslipRecordToDownload recordType, List<PaySlip> record, List<string> rowHeaders)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(recordType.ToString());
                var currentRow = 1;
                switch (recordType)
                {
                    case PayslipRecordToDownload.Payslips:
                        int rowIndex = 1;
                        foreach (var rowHead in rowHeaders)
                        {
                            worksheet.Cell(currentRow, rowIndex).Value = rowHead;
                            rowIndex++;
                        }
                        foreach (var payslip in record)
                        {
                            currentRow++;
                            int rowIndexRecord = 1;
                            foreach (var rowHead in rowHeaders)
                            {
                                worksheet.Cell(currentRow, rowIndexRecord).Value = rowHead == "Name" ?
                                    payslip.EmployeeInformation.User.FullName : rowHead == "Start Date" ?
                                    payslip.StartDate.Date.ToString() : rowHead == "End Date" ?
                                    payslip.EndDate.Date.ToString() : rowHead == "Payment Date" ?
                                    payslip.PaymentDate.Date.ToString() : rowHead == "Total Hours" ?
                                    payslip.TotalHours : rowHead == "Total Amount" ?
                                    Math.Round(payslip.TotalAmount, 2) : "No Record";
                                rowIndexRecord++;
                            }
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            var content = stream.ToArray();
                            return content;
                        }
                        break;
                    default:
                        return null;
                        break;

                }


            }
        }
    }
}
