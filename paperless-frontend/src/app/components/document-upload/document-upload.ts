import { Component, EventEmitter, Output, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { DocumentApiService } from '../../services/document-api-service';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './document-upload.html',
  styleUrls: ['./document-upload.css']
})
export class DocumentUploadComponent {
  private readonly documentService = Inject(DocumentApiService);

  @Output() uploadSuccess = new EventEmitter<void>();

  uploadForm: FormGroup;
  selectedFile: File | null = null;
  fileError: string | null = null;
  
  isSubmitting = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  // Erlaubte File-Types
  readonly allowedTypes = ['application/pdf', 'image/png', 'image/jpeg', 'application/msword', 'text/plain'];
  readonly maxFileSizeInMB = 20;

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.uploadForm = this.fb.group({
      title: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(100)
      ]]
    });
  }

  // Getter Validierungsabfragen
  get title() {
    return this.uploadForm.get('title');
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.fileError = null;

    if (!input.files || input.files.length === 0) {
      this.selectedFile = null;
      return;
    }

    const file = input.files[0];

    // Client-seitige Validierung Dateityp
    if (!this.allowedTypes.includes(file.type)) {
      this.fileError = 'Nur PDF-, PNG-, JPEG-, DOC- oder TXT-Dateien sind erlaubt.';
      this.selectedFile = null;
      input.value = '';
      return;
    }

    // Client-seitige Validierung Dateigröße
    if (file.size > this.maxFileSizeInMB * 1024 * 1024) {
      this.fileError = `Die Datei darf maximal ${this.maxFileSizeInMB} MB groß sein.`;
      this.selectedFile = null;
      input.value = '';
      return;
    }

    this.selectedFile = file;

    // Falls Titel leer, Dateinamen eintragen
    if (!this.title?.value) {
      this.uploadForm.patchValue({
        title: file.name.replace(/\.[^/.]+$/, '')
      });
    }
  }

  onSubmit(): void {
    this.successMessage = null;
    this.errorMessage = null;

    if (this.uploadForm.invalid) {
      this.uploadForm.markAllAsTouched();
      return;
    }

    if (!this.selectedFile) {
      this.fileError = 'Bitte wähle eine Datei aus.';
      return;
    }

    this.isSubmitting = true;

    // Daten als FormData für multipart/form-data
    const formData = new FormData();
    formData.append('title', this.title?.value);
    formData.append('file', this.selectedFile);

    // Aufruf geht über Nginx-Proxy an api
    this.http.post('/api/Document', formData, { responseType: 'text' }).subscribe({
      next: (response) => {
        console.log('Upload response successful:', response);
        this.successMessage = 'Dokument erfolgreich hochgeladen!';
        this.uploadForm.reset();
        this.selectedFile = null;
        this.fileError = null;
        this.isSubmitting = false;
        if (typeof (this.documentService as any)?.notifyDocumentUploaded === 'function') {
          (this.documentService as any).notifyDocumentUploaded();
        }
        this.uploadSuccess.emit();
      },
      error: (err) => {
        this.errorMessage = 'Fehler beim Hochladen. Bitte versuche es erneut.';
        console.error('Upload Error:', err);
        this.isSubmitting = false;
      }
    });
  }
}