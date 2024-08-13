import React, { useState, useEffect } from "react";
import DasboardNavbar from './DashboardNavbar';
import { useLocation } from 'react-router-dom';
import AdminPage from "./AdminPage";
import DriverPage from "./DriverPage";
import UserPage from "./UserPage";
import { jwtDecode } from 'jwt-decode';
const DashboardPage = () => {
   
    const location = useLocation();
    const [typeOfuser, setTypeUser] = useState(-1);
    const [userid, setUserId] = useState('*');
    useEffect(() => {
        const token = localStorage.getItem('token');
        const decodedToken = jwtDecode(token);
        const userDataFromToken = JSON.parse(decodedToken.sub);
        console.log('dsadsa', userDataFromToken);
        setTypeUser(userDataFromToken.TypeOfUser);
        setUserId(userDataFromToken.RowKey);
    },[]);
    const renderPage = () => {
        switch (typeOfuser) {
            case 0:
                return <AdminPage userId={userid} />;
            case 1:
                return <UserPage userId={userid} />;
                
            case 2:
                
                return <DriverPage userId={userid} />
            default:
                console.log("kita");
                return <></>;
        }
    };
   


    return (
        <div>
            
            <div>
              {renderPage()}
            </div>
          
        </div>
    );
};

export default DashboardPage;
