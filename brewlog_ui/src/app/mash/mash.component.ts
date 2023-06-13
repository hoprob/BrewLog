import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MashService } from '../bl-api';
import { LogNoteComponent } from '../log-note/log-note.component';

@Component({
  selector: 'app-mash',
  templateUrl: './mash.component.html',
  styleUrls: ['./mash.component.css']
})
export class MashComponent implements OnInit {
  @ViewChild(LogNoteComponent) logNoteComponent !:any;

  public mashForm = new FormGroup({
    measuredMashPh: new FormControl('0', [Validators.pattern('^[0-9]{1,2}\.?[0-9]{1,2}$')]),
  });

  public addedAcidForm = new FormGroup({
    addedAcid: new FormControl('0', [Validators.pattern('^[0-9]{1,2}(\.[0-9]{1,2}$)?')])
  })

  lacticAcidToAdd: number = 0;

  constructor(private router: Router, private mashService: MashService) { }

  ngOnInit(): void {
  }

  reportPh(){
    if(this.mashForm.valid){
        this.mashService.reportMashPh({
          sessionName: localStorage.getItem("currentBrewSession") ?? "",
          addPhValue: {ph: parseFloat(this.mashForm.controls.measuredMashPh.value ?? "0")}
        }).subscribe({complete: () =>{
          this.getCalculatedAdjustment();
          this.logNoteComponent.updateLog();
        }})
    }
  }

  reportAcidAdded(){
    if(this.addedAcidForm.valid){
      this.mashService.acidAddition({
      sessionName: localStorage.getItem("currentBrewSession") ?? "",
      addAcidAddition: {ml: parseFloat(this.addedAcidForm.controls.addedAcid.value ?? '0')}
      }).subscribe({
        error: error => {
          console.log(error);
        },
        complete: () =>{
          this.logNoteComponent.updateLog();
        }
      })
    }
  }

  getCalculatedAdjustment(){
    this.mashService.getPhLoweringAcid({
      sessionName: localStorage.getItem("currentBrewSession") ?? ""
    }).subscribe({next: (resp) => {
      this.lacticAcidToAdd = resp;
    }, error: (error) => {
      console.log(error);
    }})
  }

  mashComplete(){
    this.mashService.mashComplete({
      sessionName: localStorage.getItem("currentBrewSession") ?? ""
    }).subscribe({ error: (error) => {
      console.log(error);
    }, complete: () => {
      this.router.navigate(["/lauter"]);
    }})
  }

}
