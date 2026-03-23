# End-to-End Testing Plan
# Tool: Playwright or Cypress

## Test Scenarios

### 1. Student Journey - Course Registration

**Test:** Student registers, logs in, browses courses, enrolls in a course

```typescript
// tests/e2e/student-course-enrollment.spec.ts
import { test, expect } from '@playwright/test';

test('Student can register for courses', async ({ page }) => {
  // 1. Register new student
  await page.goto('http://localhost:5173/register');
  await page.fill('[name="email"]', `student${Date.now()}@example.com`);
  await page.fill('[name="password"]', 'Test@123456');
  await page.fill('[name="confirmPassword"]', 'Test@123456');
  await page.click('button[type="submit"]');
  
  // 2. Login
  await expect(page).toHaveURL(/.*login/);
  await page.fill('[name="email"]', /* saved email */);
  await page.fill('[name="password"]', 'Test@123456');
  await page.click('button[type="submit"]');
  
  // 3. Navigate to courses
  await expect(page).toHaveURL(/.*dashboard/);
  await page.click('text=Courses');
  
  // 4. Browse available courses
  await page.waitForSelector('.course-card');
  const courseCount = await page.locator('.course-card').count();
  expect(courseCount).toBeGreaterThan(0);
  
  // 5. Enroll in a course
  await page.locator('.course-card').first().click();
  await page.click('button:has-text("Enroll")');
  
  // 6. Verify enrollment
  await expect(page.locator('.success-message')).toBeVisible();
  await page.click('text=My Courses');
  await expect(page.locator('.enrolled-course')).toHaveCount(1);
  
  // 7. Check schedule updated
  await page.click('text=My Schedule');
  await expect(page.locator('.schedule-item')).toBeVisible();
});

test('Student cannot enroll in conflicting courses', async ({ page }) => {
  // Login
  await loginAsStudent(page);
  
  // Enroll in first course
  await enrollInCourse(page, 'BIL101');
  
  // Try to enroll in conflicting course (same time slot)
  await page.goto('/courses');
  await page.click('text=BIL102'); // Conflicts with BIL101
  await page.click('button:has-text("Enroll")');
  
  // Should show conflict error
  await expect(page.locator('.error-message')).toContainText('schedule conflict');
});
```

### 2. Document Submission Flow

```typescript
test('Student submits document to advisor', async ({ page }) => {
  await loginAsStudent(page);
  
  // 1. Create new document
  await page.click('text=Documents');
  await page.click('button:has-text("New Document")');
  await page.fill('[name="title"]', 'Thesis Proposal');
  await page.fill('[name="tags"]', 'thesis, research');
  await page.click('button:has-text("Create")');
  
  // 2. Upload first version
  await page.setInputFiles('input[type="file"]', 'tests/fixtures/sample.pdf');
  await page.fill('[name="notes"]', 'First draft');
  await page.click('button:has-text("Upload")');
  
  await expect(page.locator('.version-list')).toContainText('Version 1');
  
  // 3. Wait for advisor feedback (simulate)
  // In real scenario, advisor would need to comment
  
  // 4. Upload revised version
  await page.setInputFiles('input[type="file"]', 'tests/fixtures/sample-v2.pdf');
  await page.fill('[name="notes"]', 'Revised based on feedback');
  await page.click('button:has-text("Upload")');
  
  await expect(page.locator('.version-list')).toContainText('Version 2');
});
```

### 3. Advisor Review Workflow

```typescript
test('Advisor reviews and rates student documents', async ({ page }) => {
  await loginAsAdvisor(page);
  
  // 1. View assigned students
  await page.click('text=My Students');
  await expect(page.locator('.student-card')).toHaveCount(greaterThan(0));
  
  // 2. Select a student
  await page.locator('.student-card').first().click();
  
  // 3. View their documents
  await expect(page).toHaveURL(/.*student\/.*\/documents/);
  await page.locator('.document-card').first().click();
  
  // 4. Download and preview document
  await page.click('button:has-text("Preview")');
  await expect(page.locator('iframe.pdf-preview')).toBeVisible();
  
  // 5. Add comment
  await page.fill('textarea[name="comment"]', 'Good work! Please revise the conclusion.');
  await page.click('button:has-text("Submit Comment")');
  
  // 6. Rate document
  await page.fill('input[name="score"]', '85');
  await page.fill('textarea[name="ratingComments"]', 'Well structured thesis.');
  await page.click('button:has-text("Submit Rating")');
  
  await expect(page.locator('.rating-success')).toBeVisible();
});
```

### 4. Admin Management Workflow

```typescript
test('Admin assigns advisors to students', async ({ page }) => {
  await loginAsAdmin(page);
  
  // 1. Navigate to student management
  await page.click('text=Students');
  
  // 2. Filter unassigned students
  await page.click('input[name="filterUnassigned"]');
  await page.waitForSelector('.student-list');
  
  // 3. Select a student
  const unassignedStudent = page.locator('.student-row').first();
  await unassignedStudent.click();
  
  // 4. Assign advisor
  await page.click('button:has-text("Assign Advisor")');
  await page.selectOption('select[name="advisorId"]', { index: 1 });
  await page.click('button:has-text("Confirm")');
  
  // 5. Verify assignment
  await expect(page.locator('.success-notification')).toBeVisible();
  await expect(unassignedStudent.locator('.advisor-name')).not.toBeEmpty();
});

test('Admin monitors system statistics', async ({ page }) => {
  await loginAsAdmin(page);
  
  await page.click('text=Dashboard');
  
  // Verify statistics cards
  await expect(page.locator('.stat-card:has-text("Total Students")')).toBeVisible();
  await expect(page.locator('.stat-card:has-text("Total Courses")')).toBeVisible();
  await expect(page.locator('.stat-card:has-text("Total Documents")')).toBeVisible();
  
  // Check charts
  await expect(page.locator('canvas.chart')).toBeVisible();
  
  // Verify data is loaded
  const studentCount = await page.locator('.stat-card:has-text("Total Students") .count').textContent();
  expect(parseInt(studentCount || '0')).toBeGreaterThan(0);
});
```

### 5. Notification System Test

```typescript
test('Users receive real-time notifications', async ({ page, context }) => {
  // Open two browser contexts (student and advisor)
  const studentPage = await context.newPage();
  const advisorPage = await context.newPage();
  
  // Login as student
  await loginAsStudent(studentPage);
  
  // Login as advisor
  await loginAsAdvisor(advisorPage);
  
  // Student uploads document
  await studentPage.click('text=Documents');
  await studentPage.click('button:has-text("Upload")');
  // ... upload process
  
  // Advisor should receive notification
  await advisorPage.waitForSelector('.notification-badge');
  const notificationCount = await advisorPage.locator('.notification-badge').textContent();
  expect(parseInt(notificationCount || '0')).toBeGreaterThan(0);
  
  // Click notification
  await advisorPage.click('.notification-icon');
  await expect(advisorPage.locator('.notification-item')).toContainText('uploaded a document');
});
```

## Performance Benchmarks

```typescript
test('Page load times are acceptable', async ({ page }) => {
  const pages = [
    { url: '/dashboard', maxTime: 2000 },
    { url: '/courses', maxTime: 3000 },
    { url: '/documents', maxTime: 2500 },
    { url: '/schedule', maxTime: 2000 },
  ];
  
  for (const { url, maxTime } of pages) {
    const startTime = Date.now();
    await page.goto(`http://localhost:5173${url}`);
    await page.waitForLoadState('networkidle');
    const loadTime = Date.now() - startTime;
    
    expect(loadTime).toBeLessThan(maxTime);
  }
});
```

## Cross-Browser Testing

```typescript
// playwright.config.ts
export default {
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } },
    { name: 'mobile-chrome', use: { ...devices['Pixel 5'] } },
    { name: 'mobile-safari', use: { ...devices['iPhone 12'] } },
  ],
};
```

## Running E2E Tests

```bash
# Install Playwright
npm install -D @playwright/test

# Run all tests
npx playwright test

# Run specific test
npx playwright test student-course-enrollment

# Run with UI mode
npx playwright test --ui

# Run headed (see browser)
npx playwright test --headed

# Generate HTML report
npx playwright test --reporter=html
```

## Test Data Management

```typescript
// tests/fixtures/test-data.ts
export const testUsers = {
  student: {
    email: 'student@test.com',
    password: 'Test@123456',
  },
  advisor: {
    email: 'advisor@test.com',
    password: 'Test@123456',
  },
  admin: {
    email: 'admin@test.com',
    password: 'Admin@123456',
  },
};

// Setup and teardown
export async function setupTestData() {
  // Create test users, courses, etc.
}

export async function cleanupTestData() {
  // Remove test data
}
```

## CI/CD Integration

```yaml
# .github/workflows/e2e-tests.yml
name: E2E Tests

on: [push, pull_request]

jobs:
e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
   - name: Setup Node
 uses: actions/setup-node@v3
   with:
    node-version: '18'
      
      - name: Setup .NET
 uses: actions/setup-dotnet@v3
     with:
          dotnet-version: '8.0.x'
      
 - name: Install dependencies
        run: npm ci
 
      - name: Install Playwright Browsers
        run: npx playwright install --with-deps
   
      - name: Start Backend
        run: |
          dotnet run --project AdvisorySystem.Api &
          sleep 30
      
      - name: Start Frontend
        run: |
          npm run dev &
          sleep 10
      
 - name: Run E2E tests
        run: npx playwright test
   
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-report
  path: playwright-report/
```
