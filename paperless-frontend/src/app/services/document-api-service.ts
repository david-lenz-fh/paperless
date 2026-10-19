// document.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { DocumentDto, DocumentUploadDto, FileDto, DocumentUpdateDto } from '../model/document-model';

@Injectable({
  providedIn: 'root'
})

export class DocumentApiService {
  private apiUrl = '/api/Document'; 

  constructor(private http: HttpClient) { }

  private readonly refreshNeeded$ = new Subject<void>();

  get refresh$(): Observable<void> {
    return this.refreshNeeded$.asObservable();
  }

  notifyDocumentUploaded(): void {
    this.refreshNeeded$.next();
  }

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
    console.log(`${this.apiUrl}/${id}`);
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(()=>{
        this.notifyDocumentUploaded();
      })
    );
  }
  getFilesByDocumentId(documentId: number): Observable<FileDto[]> {
    return this.http.get<FileDto[]>(`${this.apiUrl}/${documentId}/files`);
  }
  updateDocument(fileDto: DocumentUpdateDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${fileDto.id}`, fileDto).pipe(
      tap(() => {
        this.notifyDocumentUploaded();
      })
    );
  }
}