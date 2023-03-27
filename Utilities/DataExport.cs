using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
//using System.IO;
using TimesheetBE.Models.IdentityModels;
using TimesheetBE.Models.InputModels;
using TimesheetBE.Utilities.Abstrctions;

namespace TimesheetBE.Utilities
{
    public class DataExport : IDataExport
    {
        public byte[] ExportAdminUsers(RecordsToDownload recordType, List<User> record)
        {
            using(var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(recordType.ToString());
                var currentRow = 1;
                switch (recordType)
                {
                    case RecordsToDownload.AdminUsers:
                        worksheet.Cell(currentRow, 1).Value = "Name";
                        worksheet.Cell(currentRow, 2).Value = "Email";
                        worksheet.Cell(currentRow, 3).Value = "Role";
                        worksheet.Cell(currentRow, 4).Value = "Status";

                        foreach(var user in record)
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = user.FullName;
                            worksheet.Cell(currentRow, 2).Value = user.Email;
                            worksheet.Cell(currentRow, 3).Value = user.Role;
                            worksheet.Cell(currentRow, 4).Value = user.IsActive ? "Active" : "Inactive";
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            var content = stream.ToArray();
                            return content;
                        }
                        break;
                    case RecordsToDownload.TeamMembers:
                        worksheet.Cell(currentRow, 1).Value = "Name";
                        worksheet.Cell(currentRow, 2).Value = "Job Title";
                        worksheet.Cell(currentRow, 3).Value = "Client Name";
                        worksheet.Cell(currentRow, 4).Value = "Payroll Type";
                        worksheet.Cell(currentRow, 5).Value = "Role";
                        worksheet.Cell(currentRow, 6).Value = "Status";

                        foreach (var user in record)
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = user.FullName;
                            worksheet.Cell(currentRow, 2).Value = user.EmployeeInformation.JobTitle;
                            worksheet.Cell(currentRow, 3).Value = user.EmployeeInformation.Client.OrganizationName;
                            worksheet.Cell(currentRow, 4).Value = user.EmployeeInformation.PayRollTypeId == 1 ? "Onshore" : "Offshore";
                            worksheet.Cell(currentRow, 5).Value = user.Role;
                            worksheet.Cell(currentRow, 6).Value = user.IsActive ? "Active" : "Inactive";
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
                        worksheet.Cell(currentRow, 1).Value = "Name";
                        worksheet.Cell(currentRow, 2).Value = "Email";
                        worksheet.Cell(currentRow, 3).Value = "Role";
                        worksheet.Cell(currentRow, 4).Value = "Phone";
                        worksheet.Cell(currentRow, 5).Value = "Invoice Schedule";
                        worksheet.Cell(currentRow, 6).Value = "Status";

                        foreach (var user in record)
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = user.FullName;
                            worksheet.Cell(currentRow, 2).Value = user.Email;
                            worksheet.Cell(currentRow, 3).Value = user.Role;
                            worksheet.Cell(currentRow, 4).Value = user.PhoneNumber;
                            worksheet.Cell(currentRow, 5).Value = user.InvoiceGenerationFrequency;
                            worksheet.Cell(currentRow, 6).Value = user.IsActive ? "Active" : "Inactive";
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
    }
}
