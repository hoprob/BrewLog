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

  constructor(private router: Router, private session:SessionService) { }

  ngOnInit(): void {
  }

  startSession() {
    this.session.createNewSession({sessionName: this.sessionName.value ?? "MySession"})
    .subscribe(resp => 
      localStorage.setItem('currentBrewSession', resp),   
    );
    this.router.navigate(["/recipe"]);
  }
}
