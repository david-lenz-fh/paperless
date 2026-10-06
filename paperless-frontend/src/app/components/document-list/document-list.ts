import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { SearchBar } from '../search-bar/search-bar';
import { DocumentApiService } from '../../services/document-api-service';
import { DocumentDto } from '../../model/document-model';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [SearchBar],
  templateUrl: './document-list.html',
  styleUrl: './document-list.css'
})
export class DocumentList implements OnInit {
  private readonly documentService = inject(DocumentApiService);

  searchTerm = signal('');
  documents = signal<DocumentDto[]>([]);

  // Berechnet gefilterte Dokumente reaktiv
  filteredDocuments = computed(() => {
    const search = this.searchTerm().toLowerCase().trim();
    const list = this.documents();

    if (!search) {
      return list;
    }

    return list.filter(doc => {
      const name = doc.filename ?? (doc as any).Filename ?? '';
      return name.toLowerCase().includes(search);
    });
  });

  ngOnInit(): void {
    this.loadDocuments();

    this.documentService.refresh$.subscribe(() => {
      this.loadDocuments();
    });
  }

  loadDocuments(): void {
    console.log('DocumentList: Reloading documents...');
    this.documentService.getAllDocuments().subscribe({
      next: docs => {
        console.log('DocumentList: Received documents from API:', docs);
        this.documents.set(docs ?? []);
      },
      error: error => {
        console.error('Failed to load documents', error);
      }
    });
  }

  onSearch(searchTerm: string): void {
    this.searchTerm.set(searchTerm);
  }
}