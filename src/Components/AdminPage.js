import React, { useState, useEffect } from "react";
import DasboardNavbar from './DashboardNavbar';
import VerificationPage from './VerificationsPage';


const AdminPage = () => {
   

    return (
        <div>
            <DasboardNavbar/>
            <div>
              <VerificationPage/>
            </div>
          
        </div>
    );
};

export default AdminPage;
