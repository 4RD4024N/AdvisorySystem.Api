# Advisory System API

🎓 Student advisory and document management system built with ASP.NET Core 8.

**Version:** 3.1.1 | **Status:** ✅ Production Ready | **Last Updated:** 2025-01-06

---

## 🚀 Quick Start

```bash
# Clone repository
git clone https://github.com/4RD4024N/AdvisorySystem.Api
cd AdvisorySystem.Api

# Restore packages
dotnet restore

# Create database
dotnet ef database update

# Run application
dotnet run

# Access Swagger UI
https://localhost:7175/swagger
```

**📖 For complete documentation:** See [QUICK_REFERENCE.md](QUICK_REFERENCE.md)

---

## ✨ Key Features

### 🔐 Authentication & Authorization
- JWT-based authentication (24-hour tokens)
- Role-based access control (Admin, Advisor, Student)
- **v3.1:** Restricted advisor permissions - own students only

### 📄 Document Management
- Create and manage documents (students)
- Version control with file uploads (PDF, DOCX, PPTX)
- Download and preview documents
- Search and filter capabilities

### 👨‍🏫 Advisor System (v3.1)
- **Admin:** Assign advisors to students
- **Advisor:** Manage own students only
  - View their documents
  - Send notifications
  - Create submissions with notes
  - View submissions
- **Student:** View assigned advisor

### 📅 Submissions (Deadlines)
- Create deadlines **by email or ID** (v3.1.1)
- Add notes to submissions
- Track submission status
- Automatic deadline notifications

### 💬 Comments & Feedback
- Comment on document versions
- Delete own comments
- View comment history

### 📊 Statistics & Reports
- Student summary statistics
- Advisor summary statistics
- Admin overview dashboard

### 🔔 Notifications
- Automatic notifications (assignments, deadlines)
- Manual notifications (single/bulk)
- Mark as read/unread
- Notification history

---

## 🛠️ Tech Stack

- **.NET 8.0** - Latest LTS framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core 8.0** - ORM with Code-First
- **SQL Server** - Database (LocalDB/Azure SQL)
- **JWT Authentication** - Secure token-based auth
- **Swagger/OpenAPI** - API documentation

**For detailed tech info:** See [TECHNOLOGY_STACK.md](TECHNOLOGY_STACK.md)

---

## 📋 API Endpoints (Summary)

### Authentication
```
POST /api/auth/login
POST /api/auth/register
```

### Documents
```
GET    /api/documents
POST   /api/documents
POST   /api/documents/{id}/versions
GET    /api/documents/download/{versionId}
GET    /api/documents/preview/{versionId}
```

### Submissions
```
GET    /api/submissions/my
POST   /api/submissions
```

### Students (Admin/Advisor)
```
GET    /api/students
GET    /api/students/{id}
POST   /api/students/{id}/send-notification
```

### Advisors
```
POST   /api/advisors/assign-to-student  (Admin)
GET    /api/advisors/my-advisor    (Student)
GET    /api/students/my-students     (Advisor)
```

**📖 Complete API Reference:** [API_DOCUMENTATION.md](API_DOCUMENTATION.md)

---

## 🔑 Default Users

| Email | Password | Role |
|-------|----------|------|
| admin@local | Admin123! | Admin |
| advisor1@local | Advisor123! | Advisor |
| student1@local | Student123! | Student |

---

## ⚙️ Configuration

**Database:** SQL Server LocalDB (Development)
```json
"ConnectionStrings": {
  "Default": "Server=(localdb)\\MSSQLLocalDB;Database=AdvisorySystemDB;..."
}
```

**JWT Settings:**
```json
"Jwt": {
  "Key": "Your-Secret-Key-32-Characters-Long",
  "ExpiresMinutes": 1440
}
```

**File Storage:**
```json
"Storage": {
  "Root": "wwwroot/uploads",
  "MaxFileSize": 104857600
}
```

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [QUICK_REFERENCE.md](QUICK_REFERENCE.md) | Quick start guide |
| [API_DOCUMENTATION.md](API_DOCUMENTATION.md) | Complete API reference |
| [ERROR_HANDLING_GUIDE.md](ERROR_HANDLING_GUIDE.md) | Troubleshooting guide |
| [ADVISOR_AUTHORIZATION_v3.1.md](ADVISOR_AUTHORIZATION_v3.1.md) | Authorization details |

---

## 🔄 Changelog

### v3.1.1 (2025-01-06) - Latest
- ✅ **Comment 403 Fix** - Advisors can now comment on student documents
- ✅ **Rating 403 Fix** - Advisors can now rate student documents (1-100 score)
- ✅ Submission creation with **email** support
- ✅ Notes field in submissions
- ✅ **Documentation cleanup** - 85% reduction (40+ files → 9 files)
- ✅ **Code cleanup** - Removed 100+ unnecessary comments
- ✅ Improved error messages and logging
- ✅ All authorization fixed to use v3.1 model (`AppUser.AdvisorId`)

### v3.1.0 (2025-01-05)
- ✅ **Restricted advisor permissions**
- ✅ Advisors can only access own students
- ✅ Admin-only endpoints added

### v3.0.0 (2024-12-20)
- ✅ Simplified advisor assignment system
- ✅ Direct student-advisor relationship
- ✅ Automatic notifications

---

## ☁️ Azure Deployment

**Recommended Services:**
- **Azure App Service** - Host Web API
- **Azure SQL Database** - Production database
- **Azure Blob Storage** - File storage
- **Application Insights** - Monitoring

**Estimated Cost:** ~$30-35/month (Basic tier)

**For deployment guide:** See Azure section in full README or contact maintainer.

---

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

---

## 📞 Contact

- **Repository:** https://github.com/4RD4024N/AdvisorySystem.Api
- **Issues:** [GitHub Issues](https://github.com/4RD4024N/AdvisorySystem.Api/issues)

---

## 📄 License

MIT License - See [LICENSE](LICENSE) file for details.

---

**🎯 Project Status:** Active Development  
**📅 Last Updated:** 2025-01-06  
**🔖 Current Version:** 3.1.1
