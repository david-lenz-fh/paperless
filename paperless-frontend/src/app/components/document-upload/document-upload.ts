import { Component, EventEmitter, Output, Input, inject, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { DocumentApiService } from '../../services/document-api-service';
import { DocumentDto, DocumentUpdateDto } from '../../model/document-model';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './document-upload.html',
  styleUrls: ['./document-upload.css']
})
export class DocumentUploadComponent {
  private readonly documentService = inject(DocumentApiService);

  @Input() documentToEdit: DocumentDto | null = null;
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

  // document update
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['documentToEdit'] && this.documentToEdit) {
      // fill title into form
      this.uploadForm.patchValue({
        title: this.documentToEdit.filename
      });
    } else if (!this.documentToEdit) {
      // normal upload, reset form
      this.uploadForm.reset();
      this.selectedFile = null;
    }
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

    console.log('3. Formular submit aufgerufen!');
    console.log('Formular Valid?:', this.uploadForm.valid);
    console.log('documentToEdit ist:', this.documentToEdit);
    this.successMessage = null;
    this.errorMessage = null;

    if (this.uploadForm.invalid) {
      this.uploadForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    // Aufruf geht über Nginx-Proxy an api
    //update
    if (this.documentToEdit) {
      const updateDto: DocumentUpdateDto = {
        id: this.documentToEdit.id,
        title: this.title?.value
      };

      this.documentService.updateDocument(updateDto).subscribe({
        next: () => {
          console.log('Update successful');
          this.successMessage = 'Dokument erfolgreich aktualisiert!';
          this.finishSubmit();
        },
        error: (err: any) => {
          this.errorMessage = 'Fehler beim Aktualisieren. Bitte versuche es erneut.';
          console.error('Update Error:', err);
          this.isSubmitting = false;
        }
      });
    // new post (upload)
    } else {
      if (!this.selectedFile) {
        this.fileError = 'Bitte wähle eine Datei aus.';
        this.isSubmitting = false;
        return;
      }

      const formData = new FormData();
      formData.append('title', this.title?.value);
      formData.append('file', this.selectedFile);

      this.http.post('/api/Document', formData, { responseType: 'text' }).subscribe({
        next: (response) => {
          console.log('Upload response successful:', response);
          this.successMessage = 'Dokument erfolgreich hochgeladen!';
          this.documentService.notifyDocumentUploaded();
          this.finishSubmit();
        },
        error: (err) => {
          this.errorMessage = 'Fehler beim Hochladen. Bitte versuche es erneut.';
          console.error('Upload Error:', err);
          this.isSubmitting = false;
        }
      });
    }
  }
  private finishSubmit(): void {
    this.uploadForm.reset();
    this.selectedFile = null;
    this.fileError = null;
    this.isSubmitting = false;
    this.uploadSuccess.emit();
  }
}