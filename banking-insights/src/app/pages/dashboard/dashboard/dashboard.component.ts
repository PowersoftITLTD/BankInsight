import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, inject, OnInit, Output } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { debounceTime, Subject, take } from 'rxjs';
import { ApiService } from '../../../services/api.service';
import { EncryptionService } from '../../../services/security/encryption.service';
import { Store } from '@ngrx/store';
import { storedDetails, userDetails } from '../../../store/auth/auth.selectors';

export interface TableColumn {
  header: string;
  field?: string;
  pipe?: string;
  pipeParams?: any;
  isNested?: boolean;
  nestedFields?: string[];
  isAction?: boolean;
  statusField?: boolean;
  sortKey?: string;
  isSwitch?: boolean;
  actionButton?: boolean;
  isNestedTree?: boolean;
  isColumnVisible?: boolean;
  isLink?: boolean;
  isDropdown?: boolean; // New property for dropdown columns
  dropdownOptions?: { label: string; value: any }[]; // Options for dropdown
  isHtml?: boolean;
  isDownload?: boolean;
  deleteActions?: boolean;
}

interface User {
  UserId: number;
  LoginName: string;
  PasswordHash: string | null;
  FirstName: string;
  LastName: string;
}




@Component({
  selector: 'app-dashboard',
  standalone: false,
  // imports: [],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})

export class DashboardComponent implements OnInit {

  constructor(private http: HttpClient, private apiService: ApiService, private store: Store) { }


  tableData: TreeNode[] | any;
  cols: any[] = [];
  teamTaskData: any;
  receivedSearchText: string = '';

  sidebarVisible = false;
  selectedRow: any = null;

userDetails: { token: string; user: string } = { token: '', user: ''};
  @Output() sidebarHide = new EventEmitter();
  private search$ = new Subject<string>();

  private security = inject(EncryptionService)

  groupedColumns = [
    {
      header: 'Bank details',
      fieldPrefix: 'dept_',
      subFields: [
        // { label: 'Closing Balance(Bank Stmt)', field: '' },
        { label: ' Balance as per bank (Refreshed @ 15/12/2025 15:48:20)', field: 'balAvailable' },
        // { label: ' Last Refreshed Date', field: 'lastTransactionDatetime' },

      ]
    },
    {
      header: 'NetSuite Details',
      fieldPrefix: 'inter_',
      subFields: [
        { label: 'Closing Balance(Bank Stmt)', field: 'closing_balance_as_per_bank_statement' },
        { label: 'Reco date', field: 'lastrecodate' },
        { label: 'Balance(Bank Book)', field: 'current_account_balance_as_per_bank_book' },
        { label: 'Info', field: '', width: 50 }
        //{ label: ' Reco date', field: '' },
      ]
    },
  ];


  bankingInsightFlatData: any = []


  ngOnInit() {


    this.store
      .select(userDetails)
      .pipe(take(1))
      .subscribe((user) => {
        this.userDetails = user
        this.getList();
        this.search$
          .pipe(
            debounceTime(500) // wait 500ms after the last keystroke
          )
          .subscribe(value => {
            this.receivedSearchText = value;
            // console.log('Debouncer called...')
          });

      });

  }


  onSearchInput(value: string) {
    this.search$.next(value); // emit to the debounced stream

    // this.receivedSearchText = value;
  }



  getList() {
    // const url = 'https://task.piplapps.com:8032/api/BankPortal/';
    // const source = 'BankPortal_Dashboard';

    const encrypted_user = this.userDetails.user
    const decrypted_user =this.security.decryptAES(encrypted_user);

    const stringify_user: User = JSON.parse(decrypted_user);


    const body = {
      UserId: stringify_user.UserId,
      BusinessGroupId: 1
    }


    const encrypted_data = this.security.encryptAES_JSON(body);

    const encrypted_body = {
      jsonEncrypt: encrypted_data
    }

    this.apiService.postDetails('BankPortal_Dashboard_PS', encrypted_body, true).subscribe({
      next: (data) => {
        const decrypted_data = this.security.decryptAES(data.data)
        const parse_table = JSON.parse(decrypted_data);
        
        const buildTree = (node: any): any => {
          // Determine the children array dynamically
          const childrenArray = node.bankAccountSummary_Ns || node.bank_Acc_Details_NS_ || [];

          // Recursively build children
          const children = childrenArray.map((child: any) => buildTree(child));

          // Decide which mapper to use
          const nodeData = node.bankAccountSummary_Ns || node.bank_Acc_Details_NS_
            ? this.mapBankApiToTreeNode(node)
            : this.mapSubsidiaryNode(node);

          // Remove nested arrays from nodeData if present
          delete nodeData.bankAccountSummary_Ns;
          delete nodeData.bank_Acc_Details_NS_;

          // Return node with children
          return children.length
            ? { data: nodeData, children }
            : { data: nodeData };
        };

        // Build the full tree from top-level subsidiaries
        const tree = parse_table.map((subsidiary: any) => buildTree(subsidiary));

        // console.log(JSON.stringify(tree, null, 2));

        this.teamTaskData = tree;

      }
    })  
  }

  mapSubsidiaryNode(subsidiary: any) {
    return {
      label: subsidiary.subsidiary,
      subId: subsidiary.subid,
      closingBalance: subsidiary.closing_balance_as_per_bank_statement,
      bookBalance: subsidiary.current_account_balance_as_per_bank_book,
      ...subsidiary
    };
  }

  mapBankApiToTreeNode(summary: any) {
    return {
      label: summary.project || 'Non-Project Accounts',
      projectId: summary.projectid,
      closingBalance: summary.closing_balance_as_per_bank_statement,
      bookBalance: summary.current_account_balance_as_per_bank_book,
      ...summary
    };
  }

  bankTransactionDetails(url: string, source: string) {
    const bankApi = `${url}${source}`;
    return this.http.get<any>(bankApi);
  }

  getList_n() {
    this.teamTaskData = this.buildBankHierarchy(this.bankingInsightFlatData, null)
  }


  private buildBankHierarchy(
    flatData: any[],
    managerId: any
  ): any[] {
    return flatData
      .filter(item => item.reportingManagerId === managerId)
      .map(item => {
        const nodeData = this.mapBankApiToTreeNode(item);
        const children = this.buildBankHierarchy(
          flatData,
          item.employeeId
        );
        return children.length
          ? { data: nodeData, children }
          : { data: nodeData };
      });
  }

  // Called by the eye icon click in the table
  onIconClick(rowData: any) {
    this.selectedRow = rowData;      // pass row data to sidebar
    this.sidebarVisible = true;      // open sidebar
  }

  // Sidebar close handler
  closeSidebar() {
    this.sidebarVisible = false;
    this.selectedRow = null;         // reset selected row
  }

  // Search bar change
  // onSearchInput(value: string) {
  //   this.receivedSearchText = value;
  // }

}
