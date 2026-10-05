import { Component, OnInit, inject } from '@angular/core';
import { SearchBar } from '../search-bar/search-bar';
import { DocumentApiService } from '../../services/document-api-service';

import { DocumentDto, DocumentUploadDto, FileDto } from '../../model/document-model';

@Component({
  selector: 'app-document-list',
  imports: [SearchBar],
  templateUrl: './document-list.html',
  styleUrl: './document-list.css'
})
export class DocumentList implements OnInit {

  private readonly documentService = inject(DocumentApiService);

  searchTerm = '';
  documents: DocumentDto[] = [];

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.documentService.getAllDocuments().subscribe({
      next: documents => {
        this.documents = documents;
      },
      error: error => {
        console.error('Failed to load documents', error);
      }
    });
  }

  get filteredDocuments(): DocumentDto[] {
    const search = this.searchTerm.toLowerCase().trim();

    if (!search) {
      return this.documents;
    }

    return this.documents.filter(document =>
      document.filename.toLowerCase().includes(search)
    );
  }

  onSearch(searchTerm: string): void {
    this.searchTerm = searchTerm;
  }
}