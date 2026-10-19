import { Component, ChangeDetectorRef, inject, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DocumentList } from './components/document-list/document-list';
import { DocumentUploadComponent } from './components/document-upload/document-upload';
import { DocumentDto } from './model/document-model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, DocumentList, DocumentUploadComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {

  private readonly cdr = inject(ChangeDetectorRef);

  @ViewChild(DocumentList) documentList?: DocumentList;
  showUploadForm = signal(false);
  
  // Signal für das aktuell gewählte Dokument zur Bearbeitung
  selectedDocument = signal<DocumentDto | null>(null);

  toggleUploadForm(): void {
    if (this.showUploadForm()) {
      this.selectedDocument.set(null);
      this.showUploadForm.set(false);
    } else {
      this.selectedDocument.set(null); 
      this.showUploadForm.set(true);
    }
  }

  onEditDocument(doc: DocumentDto): void {
    console.log('Parent hat Event empfangen:', doc);
    this.selectedDocument.set(doc);
    this.showUploadForm.set(true); 
  }

  onUploadFinished(): void {
    console.log('Upload/Update finished event received in App!');
    this.showUploadForm.set(false);
    this.selectedDocument.set(null);
    this.cdr.detectChanges();
    setTimeout(() => {
      this.documentList?.loadDocuments();
    }, 150);
  }
}