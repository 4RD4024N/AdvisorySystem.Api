# ?? Yeni Özellikler - MNK, Puanlama ve PDF Ön Ýzleme

## ?? Eklenen Özellikler Özeti

### 1. ? Öðrenci Profil Yönetimi ve MNK Kontrolü
**Amaç:** Öðrencilerin proje alabilmesi için gereken ders ve kredi koþullarýný otomatik denetleme

**Endpoint'ler:**
- `GET /api/studentprofile/me` - Profili görüntüle
- `POST /api/studentprofile` - Profil oluþtur/güncelle
- `GET /api/studentprofile/check-prerequisites` - Ön koþul kontrolü

**Özellikler:**
- Öðrenci numarasý, bölüm, GPA, tamamlanan krediler
- Kayýt tarihi takibi
- Otomatik ön koþul kontrolü
- Kredi ve ders sayýsý hesaplama

---

### 2. ? Ders Gereksinimi ve Tamamlama Takibi
**Amaç:** Hangi derslerin zorunlu olduðunu ve öðrencilerin hangi dersleri tamamladýðýný takip etme

**Endpoint'ler:**
- `GET /api/course/requirements` - Tüm ders gereksinimleri
- `POST /api/course/requirements` - Ders gereksinimi ekle (Admin)
- `GET /api/course/my-courses` - Tamamladýðým dersler
- `POST /api/course/my-courses` - Ders ekle
- `PATCH /api/course/my-courses/{id}` - Ders güncelle

**Özellikler:**
- Ders adý, kodu, kredi bilgisi
- Zorunlu/seçmeli ders ayýrýmý
- Tamamlanma durumu ve tarihi
- Not bilgisi
- Otomatik kredi hesaplama

---

### 3. ? Danýþman Deðerlendirme ve Puanlama
**Amaç:** Danýþmanlarýn öðrenci dokümanlarýný deðerlendirmesi ve puanlamasý

**Endpoint'ler:**
- `POST /api/ratings` - Puanlama yap/güncelle
- `GET /api/ratings/version/{versionId}` - Versiyon puanlarýný gör
- `GET /api/ratings/by-advisor/{advisorId}` - Danýþmanýn tüm puanlarý
- `GET /api/ratings/my-documents` - Dokümanlarýma verilen puanlar
- `DELETE /api/ratings/{id}` - Puaný sil

**Özellikler:**
- 1-100 arasý puanlama
- Detaylý yorum ekleme
- Ortalama puan hesaplama
- Birden fazla danýþman puanlayabilir
- Öðrenci kendi puanlarýný görebilir

---

### 4. ?? PDF Ön Ýzleme
**Amaç:** PDF dosyalarýný indirmeden tarayýcýda görüntüleme

**Endpoint'ler:**
- `GET /api/documents/preview/{versionId}` - PDF önizleme
- `GET /api/documents/metadata/{versionId}` - Dosya bilgileri

**Özellikler:**
- Sadece PDF dosyalarý için önizleme
- Inline file stream (tarayýcýda açýlýr)
- Dosya metadata bilgileri (boyut, tip, tarih)
- PDF.js ile entegrasyon desteði
- Otomatik dosya boyutu formatlama

---

## ?? Kullaným Senaryolarý

### Senaryo 1: Öðrenci Proje Alma Kontrolü

**Adýmlar:**
1. Öðrenci profil oluþturur
```http
POST /api/studentprofile
{
  "studentNumber": "20240001",
  "department": "Computer Science",
  "gpa": 3.75,
  "completedCredits": 120
}
```

2. Tamamladýðý dersleri ekler
```http
POST /api/course/my-courses
{
  "courseRequirementId": 1,
  "isCompleted": true,
  "grade": 85.5,
  "completionDate": "2023-06-15"
}
```

3. Ön koþul kontrolü yapar
```http
GET /api/studentprofile/check-prerequisites
```

4. Sistem yanýtý:
```json
{
  "meetsPrerequisites": true,
  "completedCredits": 120,
  "requiredCredits": 90,
  "message": "? You meet all prerequisites!"
}
```

---

### Senaryo 2: Danýþman Deðerlendirmesi

**Adýmlar:**
1. Öðrenci doküman yükler
```http
POST /api/documents/{id}/versions
(file upload)
```

2. Danýþman dokümaný görüntüler
```http
GET /api/documents/preview/12
```

3. Danýþman puanlama yapar
```http
POST /api/ratings
{
  "documentVersionId": 12,
  "score": 85,
  "comments": "Ýyi çalýþma, küçük düzeltmeler gerekli"
}
```

4. Öðrenci puanýný görür
```http
GET /api/ratings/my-documents
```

**Öðrenci Response:**
```json
[
  {
    "documentTitle": "Tez Taslaðý",
    "versionNo": 3,
    "ratings": [
      {
      "score": 85,
        "comments": "Ýyi çalýþma, küçük düzeltmeler gerekli",
        "createdAt": "2024-01-15T10:00:00Z"
      }
    ]
  }
]
```

---

### Senaryo 3: PDF Ön Ýzleme

**Frontend Kodu:**
```html
<!-- React/Vue/Angular örneði -->
<template>
  <div>
    <!-- Önce metadata ile kontrol -->
    <div v-if="canPreview">
      <!-- PDF önizleme -->
      <iframe 
        :src="`https://localhost:7175/api/documents/preview/${versionId}?token=${token}`" 
     width="100%" 
      height="600px"
        style="border: none;"
      ></iframe>
    </div>
    
    <div v-else>
      <!-- PDF deðilse sadece indir butonu -->
      <button @click="downloadFile">
        Ýndir ({{ fileSize }})
      </button>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      versionId: 12,
      canPreview: false,
      fileSize: '',
      token: localStorage.getItem('token')
    };
  },
  async mounted() {
    // Metadata çek
    const response = await fetch(`/api/documents/metadata/${this.versionId}`, {
      headers: { 'Authorization': `Bearer ${this.token}` }
    });
    const data = await response.json();
    
    this.canPreview = data.canPreview;
    this.fileSize = data.sizeFormatted;
  }
};
</script>
```

**PDF.js Ýle Geliþmiþ Önizleme:**
```javascript
import * as pdfjsLib from 'pdfjs-dist';

// PDF yükle
const loadPdf = async (versionId, token) => {
  const url = `https://localhost:7175/api/documents/preview/${versionId}`;
  
  const loadingTask = pdfjsLib.getDocument({
    url: url,
    httpHeaders: {
   'Authorization': `Bearer ${token}`
    }
  });
  
  const pdf = await loadingTask.promise;
  console.log(`PDF loaded: ${pdf.numPages} pages`);
  
  // Ýlk sayfayý render et
  const page = await pdf.getPage(1);
  const canvas = document.getElementById('pdf-canvas');
  const context = canvas.getContext('2d');
  
  const viewport = page.getViewport({ scale: 1.5 });
  canvas.width = viewport.width;
  canvas.height = viewport.height;
  
  await page.render({
    canvasContext: context,
  viewport: viewport
  }).promise;
};
```

---

## ?? Database Schema Deðiþiklikleri

### Yeni Tablolar

#### StudentProfiles
```sql
CREATE TABLE StudentProfiles (
  Id INT PRIMARY KEY IDENTITY,
  UserId NVARCHAR(450) NOT NULL,
  StudentNumber NVARCHAR(50),
  Department NVARCHAR(100),
  GPA FLOAT,
  CompletedCredits INT,
  EnrollmentDate DATETIME2,
  MeetsPrerequisites BIT DEFAULT 0,
  CreatedAt DATETIME2 NOT NULL,
  UpdatedAt DATETIME2 NOT NULL,
  FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
```

#### CourseRequirements
```sql
CREATE TABLE CourseRequirements (
  Id INT PRIMARY KEY IDENTITY,
  CourseName NVARCHAR(200) NOT NULL,
  CourseCode NVARCHAR(50),
  Credits INT NOT NULL,
  IsRequired BIT DEFAULT 1,
  Description NVARCHAR(MAX)
);
```

#### StudentCourses
```sql
CREATE TABLE StudentCourses (
  Id INT PRIMARY KEY IDENTITY,
  StudentId NVARCHAR(450) NOT NULL,
  CourseRequirementId INT NOT NULL,
  IsCompleted BIT DEFAULT 0,
  Grade FLOAT,
  CompletionDate DATETIME2,
  FOREIGN KEY (CourseRequirementId) REFERENCES CourseRequirements(Id) ON DELETE CASCADE
);
```

#### DocumentRatings
```sql
CREATE TABLE DocumentRatings (
  Id INT PRIMARY KEY IDENTITY,
  DocumentVersionId INT NOT NULL,
  AdvisorUserId NVARCHAR(450) NOT NULL,
  Score INT NOT NULL CHECK (Score >= 1 AND Score <= 100),
  Comments NVARCHAR(MAX),
  CreatedAt DATETIME2 NOT NULL,
  FOREIGN KEY (DocumentVersionId) REFERENCES DocumentVersions(Id) ON DELETE CASCADE
);
```

---

## ?? Kurulum ve Migration

### 1. Migration Oluþturma
```bash
dotnet ef migrations add AddStudentProfileAndRatingFeatures
```

### 2. Database Güncelleme
```bash
dotnet ef database update
```

### 3. Örnek Veri Ekleme (Opsiyonel)

**Ders Gereksinimleri:**
```sql
INSERT INTO CourseRequirements (CourseName, CourseCode, Credits, IsRequired, Description)
VALUES 
  ('Data Structures', 'CS201', 6, 1, 'Fundamental data structures and algorithms'),
  ('Database Systems', 'CS301', 6, 1, 'Relational database design and SQL'),
  ('Software Engineering', 'CS401', 8, 1, 'Software development methodologies'),
  ('Web Development', 'CS402', 6, 0, 'Modern web technologies');
```

---

## ?? Frontend Örnekleri

### React - Ön Koþul Kontrolü Komponenti

```jsx
import React, { useState, useEffect } from 'react';
import { Card, Progress, Alert } from '@/components/ui';

const PrerequisiteChecker = () => {
  const [check, setCheck] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
  fetchPrerequisites();
  }, []);

  const fetchPrerequisites = async () => {
    try {
      const response = await fetch('/api/studentprofile/check-prerequisites', {
        headers: {
  'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
      });
      const data = await response.json();
setCheck(data);
    } catch (error) {
      console.error('Failed to check prerequisites:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Loading...</div>;

  const percentage = (check.completedCredits / check.requiredCredits) * 100;

  return (
  <Card className="p-6">
      <h2 className="text-2xl font-bold mb-4">Proje Alma Yeterliliði</h2>
      
      <Alert variant={check.meetsPrerequisites ? 'success' : 'warning'}>
        {check.message}
      </Alert>

      <div className="mt-4">
        <div className="flex justify-between mb-2">
       <span>Tamamlanan Krediler</span>
          <span>{check.completedCredits} / {check.requiredCredits}</span>
     </div>
        <Progress value={percentage} className="h-2" />
      </div>

      <div className="mt-4 grid grid-cols-2 gap-4">
        <div>
          <p className="text-sm text-gray-500">Tamamlanan Dersler</p>
          <p className="text-2xl font-bold">{check.completedCoursesCount}</p>
  </div>
        <div>
 <p className="text-sm text-gray-500">Gerekli Dersler</p>
        <p className="text-2xl font-bold">{check.requiredCoursesCount}</p>
  </div>
      </div>

      {!check.meetsPrerequisites && (
        <div className="mt-4 p-4 bg-yellow-50 rounded">
 <p className="text-sm font-medium">
            Eksik Krediler: {check.missingCredits}
     </p>
       <p className="text-sm text-gray-600 mt-1">
            Proje alabilmek için önce gerekli dersleri tamamlamalýsýnýz.
          </p>
        </div>
      )}
    </Card>
  );
};

export default PrerequisiteChecker;
```

### React - Puanlama Komponenti

```jsx
import React, { useState } from 'react';
import { Star, Send } from 'lucide-react';

const RatingComponent = ({ versionId, onRated }) => {
  const [score, setScore] = useState(0);
  const [hoverScore, setHoverScore] = useState(0);
  const [comments, setComments] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const submitRating = async () => {
    if (score === 0) {
      alert('Lütfen puan seçin');
      return;
    }

    setSubmitting(true);
    try {
      const response = await fetch('/api/ratings', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
 },
        body: JSON.stringify({
        documentVersionId: versionId,
          score: score,
          comments: comments
    })
      });

    if (response.ok) {
        alert('Puanlama baþarýyla kaydedildi!');
        onRated && onRated();
      }
    } catch (error) {
      console.error('Failed to submit rating:', error);
      alert('Puanlama kaydedilemedi');
 } finally {
    setSubmitting(false);
    }
  };

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h3 className="text-lg font-semibold mb-4">Dokümaný Deðerlendirin</h3>
      
      {/* Yýldýz Puanlama (1-100 yerine 1-5 yýldýz gösterimi) */}
 <div className="flex items-center gap-2 mb-4">
        {[1, 2, 3, 4, 5].map((star) => {
          const scoreValue = star * 20; // 1 yýldýz = 20 puan
     return (
      <Star
              key={star}
       size={32}
        className={`cursor-pointer transition-colors ${
    (hoverScore || score) >= scoreValue
        ? 'fill-yellow-400 text-yellow-400'
        : 'text-gray-300'
}`}
       onMouseEnter={() => setHoverScore(scoreValue)}
  onMouseLeave={() => setHoverScore(0)}
          onClick={() => setScore(scoreValue)}
 />
     );
        })}
    <span className="ml-2 text-gray-600">
          {score > 0 ? `${score}/100` : 'Puan seçin'}
        </span>
      </div>

      {/* Yorum Alaný */}
      <textarea
   className="w-full border rounded p-2 mb-4"
        rows={4}
        placeholder="Deðerlendirme yorumunuzu yazýn..."
      value={comments}
        onChange={(e) => setComments(e.target.value)}
   />

      {/* Gönder Butonu */}
   <button
        onClick={submitRating}
        disabled={submitting || score === 0}
        className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600 disabled:bg-gray-300 flex items-center gap-2"
      >
        <Send size={16} />
        {submitting ? 'Kaydediliyor...' : 'Deðerlendirmeyi Gönder'}
      </button>
    </div>
  );
};

export default RatingComponent;
```

---

## ?? Test Senaryolarý

### Test 1: Ön Koþul Kontrolü

```javascript
// Test: Öðrenci profil oluþtur
const profile = {
  studentNumber: "20240001",
  department: "Computer Science",
  gpa: 3.75,
  completedCredits: 60
};

await fetch('/api/studentprofile', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
body: JSON.stringify(profile)
});

// Test: Ön koþul kontrolü
const check = await fetch('/api/studentprofile/check-prerequisites', {
  headers: { 'Authorization': `Bearer ${token}` }
}).then(r => r.json());

console.assert(!check.meetsPrerequisites, 'Should not meet prerequisites with 60 credits');
console.assert(check.missingCredits === 30, 'Should need 30 more credits');
```

### Test 2: Puanlama

```javascript
// Test: Danýþman puanlama yapar
const rating = {
  documentVersionId: 12,
  score: 85,
  comments: "Ýyi çalýþma"
};

const response = await fetch('/api/ratings', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${advisorToken}`
  },
  body: JSON.stringify(rating)
});

console.assert(response.ok, 'Rating should be created');

// Test: Öðrenci puaný görebilir
const myRatings = await fetch('/api/ratings/my-documents', {
  headers: { 'Authorization': `Bearer ${studentToken}` }
}).then(r => r.json());

console.assert(myRatings.length > 0, 'Student should see ratings');
console.assert(myRatings[0].ratings[0].score === 85, 'Score should be 85');
```

### Test 3: PDF Önizleme

```javascript
// Test: Metadata kontrolü
const metadata = await fetch('/api/documents/metadata/12', {
  headers: { 'Authorization': `Bearer ${token}` }
}).then(r => r.json());

console.assert(metadata.isPdf, 'Should be PDF file');
console.assert(metadata.canPreview, 'Should be previewable');
console.assert(metadata.previewUrl !== null, 'Should have preview URL');

// Test: PDF önizleme
const previewResponse = await fetch('/api/documents/preview/12', {
  headers: { 'Authorization': `Bearer ${token}` }
});

console.assert(previewResponse.ok, 'Preview should work');
console.assert(previewResponse.headers.get('content-type') === 'application/pdf', 'Should return PDF');
```

---

## ?? Ýlgili Dokümantasyon

- [API Documentation](API_DOCUMENTATION.md) - Tüm endpoint detaylarý
- [README](README.md) - Proje genel bakýþ
- [Use Case Summary](USE_CASE_SUMMARY.md) - Use case'ler

---

**Hazýrlayan:** Advisory System Team  
**Tarih:** 2025-01-17  
**Versiyon:** 1.1.0  
**Durum:** ? Tamamlandý ve Test Edildi
