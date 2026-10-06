import { Component, ChangeDetectorRef, inject, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DocumentList } from './components/document-list/document-list';
import { DocumentUploadComponent } from './components/document-upload/document-upload';

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


  toggleUploadForm(): void {
    this.showUploadForm.update(val => !val);
  }

  onUploadFinished(): void {
    console.log('Upload finished event received in App!');
    this.showUploadForm.set(false);
    this.cdr.detectChanges();
    setTimeout(() => {
      this.documentList?.loadDocuments();
    }, 150);
  }
}