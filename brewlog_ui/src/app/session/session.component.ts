import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { SessionService } from '../bl-api';

@Component({
  selector: 'app-session',
  templateUrl: './session.component.html',
  styleUrls: ['./session.component.css']
})
export class SessionComponent implements OnInit {

  sessionName = new FormControl("");

  sessions?: Session[];

  constructor(private router: Router, private sessionService:SessionService) { }

  ngOnInit(): void {
    this.getSessions();
  }

  startSession() {
    this.sessionService.createNewSession({sessionName: this.sessionName.value ?? "MySession"})
    .subscribe(resp => 
      localStorage.setItem('currentBrewSession', resp),   
    );
    this.router.navigate(["/recipe"]);
  }

  getSessions(){
    this.sessionService.getSessions()
    .subscribe({next: (resp: Session[]) => {
      this.sessions = resp;
    }});
  }

  continueSession(sessionName: string){
    this.sessionService.getSessionState({sessionName: sessionName})
    .subscribe({
      next: resp => {
        localStorage.setItem('currentBrewSession', sessionName);
        this.router.navigate([`/${resp.toLowerCase()}`])
      },
      error: error => {
      console.log(error);
    }});
  }
}

class Session{
  createdAt?: string;
  sessionName?: string;
}
