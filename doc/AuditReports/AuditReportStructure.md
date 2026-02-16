# Audit Report Template

Project Name: `<PROJECT_NAME>`  
Audit Date: `<YYYY-MM-DD>`  
Audit Time: `<HH:mm>`
Auditor: `AI Agent`  
Scope: `Internal Quality & Open Source Readiness`

## 1. Executive Summary
- Ringkasan kondisi project: `<RINGKASAN KONDISI>`
- Total Score (0-100): `<TOTAL_SCORE>`
- Grade (A/B/C/D): `<GRADE>`
- Apakah layak open source?: `<YA/TIDAK + ALASAN>`
- Top 5 Risk terbesar:
1. `<RISK_1>`
2. `<RISK_2>`
3. `<RISK_3>`
4. `<RISK_4>`
5. `<RISK_5>`

## 2. Score Breakdown
| Category | Score | Max Score | Status | Risk Level |
|---|---:|---:|---|---|
| Architecture | `<SCORE>` | 15 | `<STATUS>` | `<RISK_LEVEL>` |
| Code Quality | `<SCORE>` | 20 | `<STATUS>` | `<RISK_LEVEL>` |
| Security | `<SCORE>` | 20 | `<STATUS>` | `<RISK_LEVEL>` |
| Performance | `<SCORE>` | 15 | `<STATUS>` | `<RISK_LEVEL>` |
| Concurrency | `<SCORE>` | 10 | `<STATUS>` | `<RISK_LEVEL>` |
| Repository Hygiene | `<SCORE>` | 10 | `<STATUS>` | `<RISK_LEVEL>` |
| Documentation | `<SCORE>` | 10 | `<STATUS>` | `<RISK_LEVEL>` |

Total Score: `<TOTAL_SCORE>` / 100

## 3. Detailed Findings
### [CATEGORY NAME]
#### Finding #`<NOMOR>`
Severity: `<High|Medium|Low>`  
File: `<PATH_FILE>`  
Line: `<LINE_NUMBER | N/A>`  
Issue: `<RINGKASAN MASALAH>`  
Technical Explanation: `<PENJELASAN TEKNIS>`  
Impact: `<DAMPAK>`  
Risk If Ignored: `<RISIKO JIKA DIABAIKAN>`  
Recommendation: `<REKOMENDASI>`  
Suggested Refactor Code:
```csharp
// optional
```

## 4. Security Risk Analysis
- `<RISK_ANALYSIS_1>`
- `<RISK_ANALYSIS_2>`
- `<RISK_ANALYSIS_3>`

## 5. Open Source Readiness Check
- [ ] No hardcoded credentials
- [ ] Clean commit history
- [ ] License file exists
- [ ] README complete
- [ ] CONTRIBUTING.md exists
- [ ] SECURITY.md exists
- [ ] Example configuration provided
- [ ] Test coverage > 60%

## 6. Technical Debt Estimation
- Estimasi effort remediasi prioritas tinggi: `<X-Y hari kerja>`.
- Rincian:
1. `<ITEM_1>`
2. `<ITEM_2>`
3. `<ITEM_3>`

## 7. Final Recommendation
`<REKOMENDASI AKHIR>`
