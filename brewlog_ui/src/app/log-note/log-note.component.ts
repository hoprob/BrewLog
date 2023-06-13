import { Component, OnInit } from '@angular/core';
import { SessionService } from '../bl-api';
import { logNote } from '../models/logNote';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-log-note',
  templateUrl: './log-note.component.html',
  styleUrls: ['./log-note.component.css'],
})
export class LogNoteComponent implements OnInit {

  
  logNoteForm: FormGroup = new FormGroup({
    note: new FormControl('', [Validators.required]),
  });

  logNotes: logNote[] = [];
  
  displayedColumns: string[] = ['date', 'stage', 'note'];
  
  constructor(private sessionService: SessionService) {}


  ngOnInit(): void {
    this.updateLog();
  }

  submitLognoteForm(){
    if(this.logNoteForm.valid){
      this.sessionService.addLogNote({
        sessionName: localStorage.getItem('currentBrewSession') ?? '',
        addLogNote: {note: this.logNoteForm.controls['note'].value}
      }).subscribe({error: (error) => {
        console.log(error);
      }, complete: () => {
        this.updateLog();
      }})
    }
  }

  updateLog(){
    this.sessionService
      .getSessionLogNotes({
        sessionName: localStorage.getItem('currentBrewSession') ?? '',
      })
      .subscribe({
        next: (resp: logNote[]) => {
          this.logNotes = resp;
        },
        error: (error) => {
          console.log(error);
        },
      });
  }

  formatDateTimeString(dateTimeString: string): string{
    return new Date(dateTimeString).toLocaleString();
  }
}
