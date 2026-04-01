import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { OverlayPanel } from 'primeng/overlaypanel';


interface Menu {
  label: string;
  icon: string;
  hasAccess?: boolean;
  routerLink?: string | null;
  children?: Menu[];
}

@Component({
  selector: 'app-side-bar',
  standalone: false,
  // imports: [],
  templateUrl: './side-bar.component.html',
  styleUrl: './side-bar.component.scss'
})
export class SideBarComponent implements OnInit {

  @Input() docked: boolean = true;
  @Output() dockToggled = new EventEmitter<void>();

  authData: any;
  openedMenus: Set<string> = new Set();
  activeChildItems: Menu[] = [];

  @ViewChild('childOverlay') childOverlay!: OverlayPanel;

 constructor( private router: Router) {}

 ngOnInit(): void {
        // this.authData 
      this.updateMenuAccessBasedOnAttribute();
      this.updateMenuAccessBasedOnDepartment();
      this.updateMenuAccessBasedOnMilestone();
 }

   menuItems: Menu[] = [
    {
      label: 'Dashboard',
      icon: 'assets/dashboard-icon/dashboard-icon.svg',
      routerLink: '/dashboard',
      hasAccess: true,
    },
    // {
    //   label: 'Tasks',
    //   icon: 'assets/tasks.svg',
    //   routerLink: '/tasks',
    //   hasAccess: true,
    // },
    //  {
    //   label: 'Recursive Tasks',
    //   icon: 'assets/tasks.svg',
    //   routerLink: '/recursive-tasks',
    //   hasAccess: true,
    // },
    // {
    //   label: 'Templates',
    //   icon: 'assets/templates.svg',
    //   hasAccess: true,
    //   children: [
    //     { label: 'Template', icon: '', routerLink: '/template', hasAccess: true },

    //   ],
    // },
    // {
    //   label: 'Projects',
    //   icon: 'assets/projects.svg',
    //   hasAccess: true,
    //   children: [
    //     { label: 'Project Definition', icon: '', routerLink: '/project', hasAccess: true },
    //     { label: 'Document Depository', icon: '', routerLink: '/document', hasAccess: true },
    //     { label: 'Compliance', icon: '', routerLink: '/compliance', hasAccess: true },
    //   ],
    // },
    // {
    //   label:'Reports',
    //   icon:'assets/projects.svg',
    //   hasAccess:true,
    //   children:[
    //     {label:'Task Insights', icon:'', routerLink:'/reports/task-insights', hasAccess:true },
   
    //   ]
    // },
    // {
    //   label:'Management',
    //   icon:'assets/projects.svg',
    //   hasAccess:true,
    //   children:[
    //     {label:'Department view', icon:'', routerLink:'/reports/department-view', hasAccess:true },
    //   ]
    // },
    // { 
    //   label:'Milestone',
    //    icon:'assets/projects.svg',
    //    hasAccess:true,
    //    children:[
    //     {label:'upload-project', icon:'', routerLink:'/milestone/upload-project', hasAccess:true },
    //     {label:'modify-project', icon:'', routerLink:'/milestone/modify-project', hasAccess:true }

    //    ]
    // }
  ];


  toggleDock() {
    this.dockToggled.emit();
  }

  toggleSubMenu(label: string) {
    this.openedMenus.has(label)
      ? this.openedMenus.delete(label)
      : this.openedMenus.add(label);
  }

  openChildOverlay(event: MouseEvent, item: Menu) {
    if (item.children?.length) {
      this.activeChildItems = item.children.filter(child => child.hasAccess);
      this.childOverlay.toggle(event);
    }
  }

  navigateTo(link: string) {
    this.router.navigate([link]);
    this.childOverlay.hide();
  }

  updateMenuAccessBasedOnAttribute() {
    if (this.authData?.Attribute2) {
      const hasAccess = this.authData.Attribute2 === 'Y';
      this.setAccessForTemplatesAndProjects(hasAccess);
    }
  }


  

  setAccessForTemplatesAndProjects(hasAccess: boolean) {
//'Templates', 'Projects'
    [].forEach((section) => {
          // console.log('hasAccess: ', this.authData)

      const menuSection = this.menuItems.find((item) => item.label === section);
      if (menuSection) {
        menuSection.hasAccess = hasAccess;
        menuSection.children?.forEach((child) => (child.hasAccess = hasAccess));
      }
    });
  }


  updateMenuAccessBasedOnDepartment() {
    if (this.authData?.Department_View) {

      const hasAccess = this.authData.Department_View === 'N';
      this.setAccessForDepartmentView(hasAccess);
    }
  }

  setAccessForDepartmentView(hasAccess: boolean) {

//'Templates', 'Projects'
    [''].forEach((section) => {

      const menuSection = this.menuItems.find((item) => item.label === section);
      if (menuSection) {
        menuSection.hasAccess = hasAccess;
        menuSection.children?.forEach((child) => (child.hasAccess = hasAccess));
      }
    });
  }

  updateMenuAccessBasedOnMilestone() {
    if (this.authData?.Department_View) {
      const hasAccess = this.authData.Milestone_View === 'Y';
      this.setAccessForMilestone(hasAccess);
    }
  }

  setAccessForMilestone(hasAccess: boolean) {

    ['Milestone'].forEach((section) => {

      const menuSection = this.menuItems.find((item) => item.label === section);
      if (menuSection) {
        menuSection.hasAccess = hasAccess;
        menuSection.children?.forEach((child) => (child.hasAccess = hasAccess));
      }
    });
  }


}
