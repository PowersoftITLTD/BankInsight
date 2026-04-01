import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from './common/button/button.component';
import { ButtonModule } from 'primeng/button';
import { InputComponent } from './common/input/input.component';
import { PrimeTreeTableComponent } from './components/prime-tree-table/prime-tree-table.component';
import { TreeTableModule } from 'primeng/treetable';
import { LayoutComponent } from './layout/layout.component';
import { HeaderComponent } from './layout/header/header.component';
import { SideBarComponent } from './layout/side-bar/side-bar.component';
import { RouterModule } from '@angular/router';
import { SidebarModule } from 'primeng/sidebar';
import { TooltipModule } from 'primeng/tooltip';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { AdvanceSearchBarComponent } from './advance-search-bar/advance-search-bar.component';

// PrimeNG imports
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { AccordionModule } from 'primeng/accordion';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { FormsModule } from '@angular/forms';
import { SkeletonModule } from 'primeng/skeleton';
import { MspSearchPipePipe } from './pipes/msp-search/msp-search-pipe-pipe.pipe';
import { DashboardTableFilterPipe } from './pipes/tree-search/dashboard-table-filter.pipe';
import { IndianCurrencyPipe } from './pipes/currency/indian-currency.pipe';
import { ToasterContainerComponent } from './components/toaster-container/toaster-container.component';
import { ToasterComponent } from './components/toaster-container/toaster/toaster.component';



// import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
// import { BrowserModule } from '@angular/platform-browser';




@NgModule({
  declarations: [
    ButtonComponent, 
    InputComponent, 
    PrimeTreeTableComponent,
    LayoutComponent,
    HeaderComponent,
    SideBarComponent,
    AdvanceSearchBarComponent,
    MspSearchPipePipe,
    DashboardTableFilterPipe,
    IndianCurrencyPipe,
    ToasterContainerComponent,
    ToasterComponent
  ],
  imports: [
    CommonModule,
    ButtonModule,
    TreeTableModule,
    RouterModule,
    SidebarModule,
    TooltipModule,
    OverlayPanelModule,
    CheckboxModule,
    TagModule,

     // PrimeNG modules
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    DropdownModule,
    AccordionModule,
    CheckboxModule,
    SkeletonModule,
    // BrowserAnimationsModule,
    // BrowserModule, 
    
    //forms
    FormsModule 
  ],
  exports:[
    ButtonComponent,
    InputComponent,
    PrimeTreeTableComponent,
    LayoutComponent,
    HeaderComponent,
    SideBarComponent,
    AdvanceSearchBarComponent,
    MspSearchPipePipe,
    DashboardTableFilterPipe,
    IndianCurrencyPipe,
    ToasterContainerComponent
  ]
})
export class SharedModule { }
