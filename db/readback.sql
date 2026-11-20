

SELECT now() AS taken_at;

SELECT "StudentNumber", "FullName", "Contact", "IsEligible" FROM "Students" ORDER BY 1;

SELECT "EquipmentCode", "Name", "Description", "IsAvailable" FROM "Equipment" ORDER BY 1;

SELECT s."StudentNumber", e."EquipmentCode", b."BorrowedDate", b."ExpectedReturnDate", b."ReturnedDate",
       CASE WHEN b."ReturnedDate" IS NULL THEN 'Active' ELSE 'Returned' END AS status
FROM "Borrowings" b
JOIN "Students"  s ON s."Id" = b."StudentId"
JOIN "Equipment" e ON e."Id" = b."EquipmentId"
ORDER BY 1, 2, 3;

SELECT e."EquipmentCode"
FROM "Equipment" e
WHERE e."IsAvailable" = EXISTS (SELECT 1 FROM "Borrowings" b WHERE b."EquipmentId" = e."Id" AND b."ReturnedDate" IS NULL);

SELECT indexname, indexdef FROM pg_indexes WHERE tablename IN ('Students','Equipment','Borrowings') ORDER BY 1;
SELECT conname, pg_get_constraintdef(oid) FROM pg_constraint WHERE conrelid = '"Borrowings"'::regclass;
