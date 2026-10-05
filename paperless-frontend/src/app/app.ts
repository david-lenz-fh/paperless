import { Component } from '@angular/core';
import { DocumentList } from './components/document-list/document-list';

@Component({
  selector: 'app-root',
  imports: [DocumentList],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
}