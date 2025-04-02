# TESTFRAMEWORK - แผนผังการทำงานของระบบ

## การจัดการข้อมูลนักวิจัย (Researcher Management)

```mermaid
flowchart TD
    %% Main nodes
    Start([เริ่มต้นใช้งานระบบ]) --> MainMenu[หน้าหลัก]
    
    %% Main menu options
    MainMenu --> InternalList[รายชื่อนักวิจัยภายใน]
    MainMenu --> ExternalList[รายชื่อนักวิจัยภายนอก]
    
    %% Internal researchers
    InternalList --> ViewInternal[ดูข้อมูลนักวิจัยภายใน]
    InternalList --> AddInternal[เพิ่มนักวิจัยภายใน]
    InternalList --> EditInternal[แก้ไขข้อมูลนักวิจัยภายใน]
    InternalList --> DeleteInternal[ลบข้อมูลนักวิจัยภายใน]
    
    %% External researchers
    ExternalList --> ViewExternal[ดูข้อมูลนักวิจัยภายนอก]
    ExternalList --> AddExternal[เพิ่มนักวิจัยภายนอก]
    ExternalList --> EditExternal[แก้ไขข้อมูลนักวิจัยภายนอก]
    ExternalList --> DeleteExternal[ลบข้อมูลนักวิจัยภายนอก]
    
    %% Add internal researcher flow
    AddInternal --> InternalForm[กรอกแบบฟอร์มข้อมูลนักวิจัยภายใน]
    InternalForm --> ValidateInternal{ตรวจสอบข้อมูล}
    ValidateInternal -->|ข้อมูลถูกต้อง| SaveInternal[บันทึกข้อมูลลงฐานข้อมูล]
    ValidateInternal -->|ข้อมูลไม่ถูกต้อง| ShowInternalErrors[แสดงข้อผิดพลาด]
    ShowInternalErrors --> InternalForm
    SaveInternal --> InternalList
    
    %% Add external researcher flow
    AddExternal --> ExternalForm[กรอกแบบฟอร์มข้อมูลนักวิจัยภายนอก]
    ExternalForm --> ValidateExternal{ตรวจสอบข้อมูล}
    ValidateExternal -->|ข้อมูลถูกต้อง| SaveExternal[บันทึกข้อมูลลงฐานข้อมูล]
    ValidateExternal -->|ข้อมูลไม่ถูกต้อง| ShowExternalErrors[แสดงข้อผิดพลาด]
    ShowExternalErrors --> ExternalForm
    SaveExternal --> ExternalList
    
    %% Edit internal researcher flow
    EditInternal --> LoadInternalData[โหลดข้อมูลนักวิจัยภายใน]
    LoadInternalData --> EditInternalForm[แก้ไขข้อมูลในแบบฟอร์ม]
    EditInternalForm --> ValidateEditInternal{ตรวจสอบข้อมูล}
    ValidateEditInternal -->|ข้อมูลถูกต้อง| UpdateInternal[อัปเดตข้อมูลในฐานข้อมูล]
    ValidateEditInternal -->|ข้อมูลไม่ถูกต้อง| ShowEditInternalErrors[แสดงข้อผิดพลาด]
    ShowEditInternalErrors --> EditInternalForm
    UpdateInternal --> InternalList
    
    %% Edit external researcher flow
    EditExternal --> LoadExternalData[โหลดข้อมูลนักวิจัยภายนอก]
    LoadExternalData --> EditExternalForm[แก้ไขข้อมูลในแบบฟอร์ม]
    EditExternalForm --> ValidateEditExternal{ตรวจสอบข้อมูล}
    ValidateEditExternal -->|ข้อมูลถูกต้อง| UpdateExternal[อัปเดตข้อมูลในฐานข้อมูล]
    ValidateEditExternal -->|ข้อมูลไม่ถูกต้อง| ShowEditExternalErrors[แสดงข้อผิดพลาด]
    ShowEditExternalErrors --> EditExternalForm
    UpdateExternal --> ExternalList
    
    %% Delete flows
    DeleteInternal --> ConfirmDeleteInternal{ยืนยันการลบ?}
    ConfirmDeleteInternal -->|ยืนยัน| RemoveInternal[ลบข้อมูลจากฐานข้อมูล]
    ConfirmDeleteInternal -->|ยกเลิก| InternalList
    RemoveInternal --> InternalList
    
    DeleteExternal --> ConfirmDeleteExternal{ยืนยันการลบ?}
    ConfirmDeleteExternal -->|ยืนยัน| RemoveExternal[ลบข้อมูลจากฐานข้อมูล]
    ConfirmDeleteExternal -->|ยกเลิก| ExternalList
    RemoveExternal --> ExternalList
    
    %% Styling
    classDef mainNode fill:#4CAF50,stroke:#388E3C,color:white,stroke-width:2px
    classDef formNode fill:#2196F3,stroke:#0D47A1,color:white,stroke-width:2px
    classDef decisionNode fill:#FFC107,stroke:#FF8F00,color:black,stroke-width:2px
    classDef actionNode fill:#9C27B0,stroke:#6A1B9A,color:white,stroke-width:2px
    classDef errorNode fill:#F44336,stroke:#B71C1C,color:white,stroke-width:2px
    
    class Start,MainMenu,InternalList,ExternalList mainNode
    class InternalForm,ExternalForm,EditInternalForm,EditExternalForm formNode
    class ValidateInternal,ValidateExternal,ValidateEditInternal,ValidateEditExternal,ConfirmDeleteInternal,ConfirmDeleteExternal decisionNode
    class SaveInternal,SaveExternal,UpdateInternal,UpdateExternal,RemoveInternal,RemoveExternal actionNode
    class ShowInternalErrors,ShowExternalErrors,ShowEditInternalErrors,ShowEditExternalErrors errorNode
```

## โครงสร้างข้อมูล (Data Structure)

```mermaid
flowchart TD
    %% Main entities
    Researcher[นักวิจัย\nResearcher_tbl]
    WorkGroup[กลุ่มงาน\nwork_groups]
    Department[ฝ่ายงาน\ndepartments]
    Division[แผนก\ndivisions]
    TypeResearch[ประเภทนักวิจัย\nTypeResearch]
    
    %% Relationships
    Researcher -->|สังกัด| WorkGroup
    Researcher -->|สังกัด| Department
    Researcher -->|สังกัด| Division
    Researcher -->|มีประเภทเป็น| TypeResearch
    
    Department -->|อยู่ภายใต้| WorkGroup
    Division -->|อยู่ภายใต้| Department
    
    %% Styling
    classDef entityNode fill:#3F51B5,stroke:#1A237E,color:white,stroke-width:2px,rx:10px,ry:10px
    class Researcher,WorkGroup,Department,Division,TypeResearch entityNode
```

## การแก้ไขปัญหาการแสดงคำนำหน้าชื่อ (Title Display Fix)

```mermaid
flowchart LR
    %% Problem and solution
    Problem[ปัญหา:\nคำนำหน้าชื่อไม่แสดงผล]
    Cause[สาเหตุ:\nไม่มีเมธอด POST\nสำหรับ EditInternal\nและ EditExternal]
    Effect[ผลกระทบ:\nข้อมูลคำนำหน้าไม่ถูกบันทึก\nลงฐานข้อมูล]
    
    Solution[การแก้ไข:\nเพิ่มเมธอด POST]
    Process[กระบวนการ:\nตรวจสอบและบันทึก\nข้อมูลคำนำหน้า]
    Result[ผลลัพธ์:\nคำนำหน้าชื่อแสดงผล\nได้อย่างถูกต้อง]
    
    %% Flow
    Problem --> Cause --> Effect
    Solution --> Process --> Result
    
    %% Styling
    classDef problemNode fill:#F44336,stroke:#B71C1C,color:white,stroke-width:2px,rx:5px,ry:5px
    classDef solutionNode fill:#4CAF50,stroke:#388E3C,color:white,stroke-width:2px,rx:5px,ry:5px
    
    class Problem,Cause,Effect problemNode
    class Solution,Process,Result solutionNode
```

## ขั้นตอนการทำงานของระบบ (System Workflow)

```mermaid
flowchart TD
    %% Main workflow
    User([ผู้ใช้งาน]) --> Browser[เว็บเบราว์เซอร์]
    Browser --> |HTTP Request| Controller[Controller\nจัดการคำร้องขอ]
    
    Controller --> |GET| View[View\nแสดงผลหน้าเว็บ]
    Controller --> |POST| Model[Model\nจัดการข้อมูล]
    
    Model --> |ดึงข้อมูล/บันทึกข้อมูล| Database[(ฐานข้อมูล\nSQL Server)]
    Model --> |ส่งข้อมูล| Controller
    
    Controller --> |ส่งข้อมูล| View
    View --> |HTTP Response| Browser
    Browser --> User
    
    %% Styling
    classDef userNode fill:#FF9800,stroke:#E65100,color:white,stroke-width:2px,rx:30px,ry:30px
    classDef componentNode fill:#3F51B5,stroke:#1A237E,color:white,stroke-width:2px
    classDef dataNode fill:#009688,stroke:#004D40,color:white,stroke-width:2px,rx:5px,ry:5px
    
    class User userNode
    class Browser,Controller,View componentNode
    class Model,Database dataNode
```
