import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LauterService } from '../bl-api';

@Component({
  selector: 'app-lauter',
  templateUrl: './lauter.component.html',
  styleUrls: ['./lauter.component.css']
})
export class LauterComponent implements OnInit {

  lauterForm = new FormGroup({
    waterInLauter: new FormControl(0, [Validators.pattern('^[0-9]{1,2}\.?[0-9]{1,2}$'), Validators.min(1), Validators.required]),
  });

  constructor(private router: Router, private lauterService: LauterService) { }

  ngOnInit(): void {
  }

  submitLauterForm(){
    if(this.lauterForm.valid){
      this.lauterService.totalWaterInLauter({
        sessionName: localStorage.getItem("currentBrewSession") ?? "",
        addTotalWaterInLauter: {litresWater: this.lauterForm.controls.waterInLauter.value ?? 0}
      }).subscribe({error: (error) => {
        console.log(error);
      }, complete: () => {
        this.router.navigate(["/boil"]);
      }})
    } 
  }

}
