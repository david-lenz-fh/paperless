// document.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DocumentDto, DocumentUploadDto, FileDto } from '../model/document-model';

@Injectable({
  providedIn: 'root'
})
export class DocumentApiService {
  private apiUrl = 'http://localhost:8080/api/Document'; 

  constructor(private http: HttpClient) { }

  getAllDocuments(): Observable<DocumentDto[]> {
    return this.http.get<DocumentDto[]>(this.apiUrl);
  }

  getDocumentById(id: number): Observable<DocumentDto> {
    return this.http.get<DocumentDto>(`${this.apiUrl}/${id}`);
  }
  uploadDocument(fileDto: DocumentUploadDto): Observable<void> {
    return this.http.post<void>(this.apiUrl, fileDto);
  }
  deleteDocument(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
  getFilesByDocumentId(documentId: number): Observable<FileDto[]> {
    return this.http.get<FileDto[]>(`${this.apiUrl}/${documentId}/files`);
  }
}