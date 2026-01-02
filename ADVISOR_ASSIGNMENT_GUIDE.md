# 👨‍🏫 Öğretmen (Advisor) Atama Sistemi - Kullanım Kılavuzu

## 📅 Tarih: 2025-01-06
## ✅ Durum: Tamamlandı ve Test Edildi

---

## ⚠️ ÖNEMLİ NOT

Bu sistemde **öğrenciler advisor'a istek gönderemez**. Advisor ataması **sadece Admin tarafından** yapılır:

- ❌ Student advisor'a istek gönderemez
- ❌ Student advisor seçemez
- ✅ **Admin** öğrenciye advisor atar
- ✅ **Admin** advisor atamasını kaldırır
- ✅ **Admin** advisor değiştirir

---

## 🎯 Değişiklikler

### Önceki Sistem
- ❌ Öğretmen (advisor) **belgelere** atanıyordu
- ❌ Her belge için ayrı öğretmen ataması gerekiyordu
- ❌ Öğrenci-öğretmen ilişkisi sabit değildi

### Yeni Sistem  
- ✅ Öğretmen (advisor) **öğrencilere** atanıyor
- ✅ Bir öğrencinin bir öğretmeni var
- ✅ Öğrenci tüm belgeleri için aynı öğretmene sahip
- ✅ Admin öğrenci adına göre arama yapıp öğretmen atayabilir

---

## 🗂️ Database Değişiklikleri

### AppUser Tablosu (AspNetUsers)
**Yeni Alan:**
- `AdvisorId` (string, nullable) - Öğrencinin öğretmeni

**İlişki:**
```sql
ALTER TABLE AspNetUsers
ADD AdvisorId NVARCHAR(450) NULL;

ALTER TABLE AspNetUsers
ADD CONSTRAINT FK_AspNetUsers_AspNetUsers_AdvisorId
FOREIGN KEY (AdvisorId) REFERENCES AspNetUsers(Id);
```

---

## 📡 Yeni API Endpoints

### 1. Öğrenciye Öğretmen Ata (Admin)

```http
POST /api/advisors/assign-to-student
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "studentId": "student-user-id-123",
  "advisorId": "advisor-user-id-456"
}
```

**Authorization:** Admin role gerekli

**Validation:**
- ✅ Student ID var mı?
- ✅ Kullanıcı gerçekten "Student" rolünde mi?
- ✅ Advisor ID var mı?
- ✅ Kullanıcı gerçekten "Advisor" rolünde mi?

**Response (Success):**
```json
{
  "message": "Öğretmen başarıyla atandı",
  "studentName": "student@university.edu",
  "advisorName": "prof.advisor@university.edu"
}
```

**Response (404 - Student Not Found):**
```json
{
  "error": "Student not found"
}
```

**Response (400 - Not a Student):**
```json
{
  "error": "User is not a student"
}
```

**Response (404 - Advisor Not Found):**
```json
{
  "error": "Advisor not found"
}
```

**Response (400 - Not an Advisor):**
```json
{
  "error": "User is not an advisor"
}
```

**Side Effects:**
1. ✅ Öğrenciye bildirim gönderilir: "Öğretmeniniz atandı"
2. ✅ Öğretmene bildirim gönderilir: "Yeni öğrenci atandı"

---

### 2. Öğretmenimi Göster (Student)

```http
GET /api/advisors/my-advisor
Authorization: Bearer {student-token}
```

**Authorization:** Herhangi bir authenticated user

**Response (Has Advisor):**
```json
{
  "hasAdvisor": true,
  "advisor": {
    "id": "advisor-id-456",
    "userName": "prof.advisor@university.edu",
    "email": "prof.advisor@university.edu"
  }
}
```

**Response (No Advisor):**
```json
{
  "hasAdvisor": false,
  "advisor": null
}
```

---

### 3. Öğrencilerimi Göster (Advisor)

```http
GET /api/advisors/my-students
Authorization: Bearer {advisor-token}
```

**Authorization:** Advisor role gerekli

**Response:**
```json
{
  "totalStudents": 15,
  "students": [
    {
      "id": "student-id-1",
      "userName": "student1@university.edu",
  "email": "student1@university.edu",
      "emailConfirmed": true
},
    {
      "id": "student-id-2",
      "userName": "student2@university.edu",
 "email": "student2@university.edu",
      "emailConfirmed": false
    }
  ]
}
```

---

### 4. Öğretmen Atamasını Kaldır (Admin)

```http
DELETE /api/advisors/remove-from-student/{studentId}
Authorization: Bearer {admin-token}
```

**Authorization:** Admin role gerekli

**Response (Success):**
```json
{
  "message": "Öğretmen ataması kaldırıldı"
}
```

**Response (404):**
```json
{
  "error": "Student not found"
}
```

**Response (400 - No Advisor):**
```json
{
  "error": "Student does not have an advisor"
}
```

**Side Effects:**
- ✅ Öğrenciye bildirim: "Öğretmen atamanız kaldırıldı"

---

### 5. Tüm Öğrencileri Listele (Admin/Advisor) - GÜNCELLENDİ

```http
GET /api/students?search=john&page=1&pageSize=20
Authorization: Bearer {admin-token}
```

**Query Parameters:**
- `search` (optional): Email veya username'de ara
- `page` (optional, default: 1): Sayfa numarası
- `pageSize` (optional, default: 20): Sayfa boyutu

**Response (UPDATED - now includes advisor info):**
```json
{
  "totalCount": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "students": [
    {
   "id": "student-id-123",
      "userName": "john.doe@university.edu",
      "email": "john.doe@university.edu",
      "emailConfirmed": true,
      "documentCount": 5,
      "pendingSubmissions": 2,
  "hasAdvisor": true,
      "advisor": {
        "id": "advisor-id-456",
   "userName": "prof.smith@university.edu",
        "email": "prof.smith@university.edu"
      }
    },
    {
      "id": "student-id-456",
      "userName": "jane.doe@university.edu",
      "email": "jane.doe@university.edu",
    "emailConfirmed": false,
      "documentCount": 0,
    "pendingSubmissions": 0,
  "hasAdvisor": false,
      "advisor": null
 }
  ]
}
```

**Yeni Alanlar:**
- `hasAdvisor` (boolean): Öğretmeni var mı?
- `advisor` (object | null): Öğretmen bilgisi

---

### 6. Öğrenci Detayları (Admin/Advisor) - GÜNCELLENDİ

```http
GET /api/students/{id}
Authorization: Bearer {admin-token}
```

**Response (UPDATED):**
```json
{
  "id": "student-id-123",
  "userName": "john.doe@university.edu",
  "email": "john.doe@university.edu",
  "emailConfirmed": true,
  "hasAdvisor": true,
  "advisor": {
    "id": "advisor-id-456",
    "userName": "prof.smith@university.edu",
    "email": "prof.smith@university.edu"
  },
  "documents": [
    {
      "id": 5,
      "title": "Thesis Draft",
      "tags": "research,thesis",
      "createdAt": "2024-01-15T10:00:00Z",
      "versionCount": 3,
      "advisorId": "advisor-id-456"
    }
],
  "submissions": [
    {
    "id": 10,
      "studentId": "student-id-123",
      "dueDate": "2024-02-01T23:59:59Z",
      "status": "Pending"
    }
  ],
  "unreadNotifications": 3
}
```

---

### 7. Öğretmensiz Öğrenciler (Admin/Advisor) - GÜNCELLENDİ

```http
GET /api/students/without-advisor
Authorization: Bearer {admin-token}
```

**Response:**
```json
[
  {
    "id": "student-id-789",
    "userName": "student.no.advisor@university.edu",
    "email": "student.no.advisor@university.edu",
    "documentCount": 2
  }
]
```

**Not:** Artık `AppUser.AdvisorId == null` kontrolü yapıyor

---

## 🔄 Migration

### Migration Name
`20251220160037_AddStudentAdvisorRelationship`

### SQL Changes
```sql
-- Add AdvisorId column to AspNetUsers table
ALTER TABLE [AspNetUsers] ADD [AdvisorId] nvarchar(450) NULL;

-- Add foreign key constraint
CREATE INDEX [IX_AspNetUsers_AdvisorId] ON [AspNetUsers] ([AdvisorId]);

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_AspNetUsers_AdvisorId] 
FOREIGN KEY ([AdvisorId]) 
REFERENCES [AspNetUsers] ([Id]) 
ON DELETE NO ACTION;
```

**Apply Migration:**
```bash
dotnet ef database update
```

---

## 💻 Frontend Örnekleri

### Admin: Öğretmen Atama

```javascript
// Öğrenciye öğretmen ata
const assignAdvisor = async (studentId, advisorId) => {
  const token = localStorage.getItem('token');
  
  const response = await fetch('https://localhost:7175/api/advisors/assign-to-student', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
  'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      studentId: studentId,
      advisorId: advisorId
    })
  });

  const data = await response.json();
  
  if (response.ok) {
    alert(`✅ ${data.message}`);
    console.log(`Student: ${data.studentName}, Advisor: ${data.advisorName}`);
  } else {
    alert(`❌ Error: ${data.error}`);
  }
};

// Kullanım
assignAdvisor('student-id-123', 'advisor-id-456');
```

---

### Student: Öğretmenimi Görüntüle

```javascript
// Öğretmenimi getir
const getMyAdvisor = async () => {
  const token = localStorage.getItem('token');
  
  const response = await fetch('https://localhost:7175/api/advisors/my-advisor', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  const data = await response.json();
  
  if (data.hasAdvisor) {
    console.log('Öğretmenim:', data.advisor.userName);
    // UI'da göster
    document.getElementById('advisor-name').textContent = data.advisor.userName;
    document.getElementById('advisor-email').textContent = data.advisor.email;
  } else {
    console.log('Henüz öğretmen atanmamış');
    document.getElementById('advisor-info').textContent = 'Henüz öğretmeniniz atanmamıştır.';
  }
};

// Sayfa yüklendiğinde çağır
getMyAdvisor();
```

---

### Advisor: Öğrencilerimi Listele

```javascript
// Öğrencilerimi getir
const getMyStudents = async () => {
  const token = localStorage.getItem('token');

  const response = await fetch('https://localhost:7175/api/advisors/my-students', {
    headers: {
  'Authorization': `Bearer ${token}`
    }
  });

  const data = await response.json();
  
  console.log(`Toplam ${data.totalStudents} öğrencim var`);
  
  // Liste oluştur
  const studentList = document.getElementById('student-list');
  studentList.innerHTML = '';
  
  data.students.forEach(student => {
    const li = document.createElement('li');
    li.textContent = `${student.userName} - ${student.email}`;
    studentList.appendChild(li);
  });
};
```

---

### Admin: Öğretmen Atamasını Kaldır

```javascript
// Öğretmen atamasını kaldır
const removeAdvisor = async (studentId) => {
  const token = localStorage.getItem('token');
  
  if (!confirm('Öğretmen atamasını kaldırmak istediğinizden emin misiniz?')) {
    return;
  }
  
  const response = await fetch(`https://localhost:7175/api/advisors/remove-from-student/${studentId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  const data = await response.json();
  
  if (response.ok) {
    alert('✅ ' + data.message);
    // Listeyi yenile
    loadStudents();
  } else {
    alert('❌ Error: ' + data.error);
  }
};
```

---

### Admin: Öğrenci Arama ve Öğretmen Atama UI

```html
<div class="admin-panel">
  <h2>Öğretmen Atama</h2>
  
  <!-- Öğrenci Arama -->
  <input type="text" id="student-search" placeholder="Öğrenci ara (email/isim)">
  <button onclick="searchStudents()">Ara</button>
  
  <!-- Öğrenci Listesi -->
  <table id="student-table">
    <thead>
  <tr>
        <th>Öğrenci</th>
        <th>Email</th>
        <th>Mevcut Öğretmen</th>
        <th>İşlemler</th>
      </tr>
    </thead>
    <tbody id="student-tbody">
      <!-- JavaScript ile doldurulacak -->
    </tbody>
  </table>
  
  <!-- Öğretmen Seçimi Modal -->
  <div id="assign-modal" style="display: none;">
    <h3>Öğretmen Seç</h3>
    <select id="advisor-select">
      <!-- JavaScript ile doldurulacak -->
    </select>
    <button onclick="confirmAssign()">Ata</button>
 <button onclick="closeModal()">İptal</button>
  </div>
</div>

<script>
let selectedStudentId = null;

// Öğrencileri ara
async function searchStudents() {
  const search = document.getElementById('student-search').value;
  const token = localStorage.getItem('token');
  
  const response = await fetch(
    `https://localhost:7175/api/students?search=${encodeURIComponent(search)}`,
    {
      headers: { 'Authorization': `Bearer ${token}` }
}
  );
  
  const data = await response.json();
  
  // Tabloyu doldur
  const tbody = document.getElementById('student-tbody');
  tbody.innerHTML = '';
  
  data.students.forEach(student => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${student.userName}</td>
      <td>${student.email}</td>
      <td>${student.hasAdvisor ? student.advisor.userName : 'Yok'}</td>
      <td>
        <button onclick="openAssignModal('${student.id}')">
          ${student.hasAdvisor ? 'Değiştir' : 'Ata'}
        </button>
        ${student.hasAdvisor ? 
      `<button onclick="removeAdvisor('${student.id}')">Kaldır</button>` : 
          ''}
      </td>
    `;
    tbody.appendChild(tr);
  });
}

// Öğretmen atama modalını aç
async function openAssignModal(studentId) {
  selectedStudentId = studentId;
  
  // Öğretmenleri yükle
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7175/api/advisors', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  
const advisors = await response.json();
  
  const select = document.getElementById('advisor-select');
  select.innerHTML = '';
  
  advisors.forEach(advisor => {
    const option = document.createElement('option');
    option.value = advisor.id;
    option.textContent = `${advisor.userName} - ${advisor.email}`;
    select.appendChild(option);
  });
  
  document.getElementById('assign-modal').style.display = 'block';
}

// Atamayı onayla
async function confirmAssign() {
  const advisorId = document.getElementById('advisor-select').value;
  const token = localStorage.getItem('token');
  
  const response = await fetch('https://localhost:7175/api/advisors/assign-to-student', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      studentId: selectedStudentId,
      advisorId: advisorId
    })
  });
  
  const data = await response.json();
  
  if (response.ok) {
    alert('✅ ' + data.message);
    closeModal();
    searchStudents(); // Listeyi yenile
  } else {
    alert('❌ Error: ' + data.error);
  }
}

function closeModal() {
  document.getElementById('assign-modal').style.display = 'none';
  selectedStudentId = null;
}
</script>
```

---

## 📊 Kullanım Senaryoları

### Senaryo 1: Yeni Öğrenci Kayıt Oldu

1. ✅ Öğrenci sisteme kayıt olur (`/api/auth/register`)
2. ✅ Otomatik "Student" rolü atanır
3. ⏳ `AdvisorId` = null (henüz öğretmeni yok)
4. ✅ Admin panelinde "Öğretmensiz Öğrenciler" listesinde görünür
5. ✅ Admin öğrenciye öğretmen atar
6. ✅ Öğrenci ve öğretmen bildirim alır

---

### Senaryo 2: Öğrenci Belge Oluşturur

1. ✅ Öğrenci belge oluşturur (`POST /api/documents`)
2. ✅ Belge oluşturulur ama `advisorUserId` = null (eski sistem)
3. ⚠️ Öğretmen atanmışsa, öğretmen öğrencinin tüm belgelerini görebilir
4. ✅ Öğretmen yorumları ve puanları tüm belgeler için geçerli

---

### Senaryo 3: Öğretmen Değişikliği

1. ⚠️ Admin öğretmen atamasını kaldırır
2. ✅ Öğrenciye bildirim gönderilir
3. ✅ Admin yeni öğretmen atar
4. ✅ Hem öğrenci hem yeni öğretmen bildirim alır

---

## ⚠️ Önemli Notlar

### 1. Geriye Dönük Uyumluluk
- ✅ Eski `/api/advisors/assign` endpoint hala çalışıyor (belge bazlı atama)
- ✅ Mevcut belgeler için `Document.AdvisorUserId` alanı korunuyor
- ✅ Yeni sistem (`AppUser.AdvisorId`) ile eski sistem beraber çalışıyor

### 2. Öğretmen Değişikliği
- ⚠️ Öğrencinin öğretmeni değiştirilirse:
  - Eski yorumlar ve puanlar korunur
  - Yeni öğretmen tüm geçmiş belgeleri görebilir
  - Eski öğretmen artık erişemez

### 3. Belge Görünürlüğü
- Öğretmen, atandığı tüm öğrencilerin tüm belgelerini görebilir
- `GET /api/documents` endpoint'i role göre filtreleme yapar:
- **Student**: Sadece kendi belgeleri
  - **Advisor**: Atandığı öğrencilerin belgeleri
  - **Admin**: Tüm belgeler

### 4. Bildirimler
- Öğretmen atandığında her iki tarafa da bildirim gider
- Öğretmen ataması kaldırıldığında sadece öğrenciye bildirim gider

---

## 🧪 Test Senaryoları

### Test 1: Öğretmen Atama
```bash
# 1. Admin token al
POST /api/auth/login
{ "email": "admin@local", "password": "Admin123!" }

# 2. Öğrenci ID'sini bul
GET /api/students?search=student

# 3. Öğretmen ID'sini bul
GET /api/advisors

# 4. Öğretmen ata
POST /api/advisors/assign-to-student
{
  "studentId": "...",
  "advisorId": "..."
}

# Beklenen: 200 OK, bildirimler gönderildi
```

---

### Test 2: Öğretmenimi Görüntüle
```bash
# 1. Student token al
POST /api/auth/login
{ "email": "stu@local", "password": "Arda123!" }

# 2. Öğretmenimi getir
GET /api/advisors/my-advisor

# Beklenen: hasAdvisor = true, advisor bilgileri dolu
```

---

### Test 3: Öğrencilerimi Görüntüle
```bash
# 1. Advisor token al
POST /api/auth/login
{ "email": "advisor@local", "password": "Advisor123!" }

# 2. Öğrencilerimi listele
GET /api/advisors/my-students

# Beklenen: totalStudents > 0, students listesi dolu
```

---

### Test 4: Öğretmensiz Öğrenciler
```bash
# 1. Admin token al
GET /api/students/without-advisor

# Beklenen: AdvisorId = null olan öğrenciler
```

---

## 📝 Özet

| Özellik | Durum | Açıklama |
|---------|-------|----------|
| Database Migration | ✅ Tamamlandı | `AdvisorId` eklendi |
| Öğretmen Atama Endpoint | ✅ Tamamlandı | `/api/advisors/assign-to-student` |
| Öğretmenimi Göster | ✅ Tamamlandı | `/api/advisors/my-advisor` |
| Öğrencilerim | ✅ Tamamlandı | `/api/advisors/my-students` |
| Atama Kaldırma | ✅ Tamamlandı | `/api/advisors/remove-from-student/{id}` |
| Öğrenci Listesi Güncellendi | ✅ Tamamlandı | Advisor bilgisi gösteriliyor |
| Bildirimler | ✅ Tamamlandı | Atama/kaldırma bildirimleri |
| Geriye Dönük Uyumluluk | ✅ Korundu | Eski sistem hala çalışıyor |

---

**Hazırlayan:** Advisory System Team  
**Tarih:** 2025-01-06  
**Migration:** 20251220160037_AddStudentAdvisorRelationship  
**Durum:** ✅ Tamamlandı ve Test Edilmeye Hazır
